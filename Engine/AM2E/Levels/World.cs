using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AM2E.Actors;
using AM2E.Collision;
using AM2E.Graphics;
using GameContent;
using Newtonsoft.Json;

namespace AM2E.Levels;

public static class World
{
    private static LDtkWorldInstance world;
    private static readonly Dictionary<string, LDtkLevelInstance> ldtkLevels = new();
    private static readonly Dictionary<int, Tileset> Tilesets = new();
    public static Dictionary<string, Level> LoadedLevels = new();
    public static Dictionary<string, Level> ActiveLevels = new();
    private static bool inTick = false;
    private static List<Level> levelsToBeActivated = new();
    private static List<Level> levelsToBeDeactivated = new();
    private static readonly List<LDtkLevelInstance> levelsToBeInstantiated = new();
    private static readonly List<Level> levelsToBeUninstantiated = new();

    public static int LevelUnitHeight => world.WorldGridHeight;
    public static int LevelUnitWidth => world.WorldGridWidth;

    public static void LoadWorld(string path)
    {
        Tilesets.Clear();
        LoadedLevels.Clear();
        ActiveLevels.Clear();
        levelsToBeActivated.Clear();
        levelsToBeDeactivated.Clear();
        ldtkLevels.Clear();
        
        JsonSerializer serializer = new();
        using (var reader = File.OpenText(path))
        {
            world = (LDtkWorldInstance)serializer.Deserialize(reader, typeof(LDtkWorldInstance));
        }

        // Load tileset definitions.
        foreach (var tileset in world.Defs.Tilesets)
        {
            // TODO: Throw nicer error message here when RelPath is invalid... and when this doesn't match an enum... etc.
            var entries = tileset.RelPath.Split('/');
            Enum.TryParse(entries[^2], out PageIndex pageIndex);
            Enum.TryParse(tileset.Identifier, out SpriteIndex spriteIndex);
            var sprite = TextureManager.GetPage(pageIndex).Sprites[spriteIndex];
            
            Tilesets.Add(tileset.Uid, new Tileset(sprite, tileset));
        }

        // Load level data.
        // TODO: Load these from files dynamically? Holding everything in memory all at once is probably kind of bad...
        foreach (var level in world.Levels)
        {
            using var reader = File.OpenText("worlds/" + level.ExternalRelPath);
            var instance = (LDtkLevelInstance)serializer.Deserialize(reader, typeof(LDtkLevelInstance));
            ldtkLevels.Add(instance.Iid, instance);
        }
    }

    public static void InstantiateLevel(string id)
    {
        // TODO: Review instantiation here for security
        
        var level = ldtkLevels[id];

        if (LoadedLevels.ContainsKey(level.Iid))
            return;
        
        
        if (inTick)
        {
            QueueLevelForInstantiation(level);
            return;
        }
        
        LoadedLevels.Add(level.Iid, new Level(level));
        
        foreach (var ldtkLayer in level.LayerInstances.Reverse())
        {
            // Create layer if it doesn't already exist.
            var layer = LoadedLevels[level.Iid].AddLayer(ldtkLayer.Identifier);
            
            // TODO: We may need to have a means of intentionally choosing *not* to instantiate a layer.
            
            switch (ldtkLayer.Type)
            {
                // TODO: asset layers :)
                case LDtkLayerType.Entities:
                    foreach (var entity in ldtkLayer.EntityInstances)
                    {
                        var entityType = Type.GetType("GameContent." + entity.Identifier);
                        // TODO: CreateInstance may be a major bottleneck when given args here :(
                        var ent = Activator.CreateInstance(entityType, entity, level.WorldX + entity.Px[0], level.WorldY + entity.Px[1], layer);
                        switch (ent)
                        {
                            case Actor actor:
                                ActorManager.Instantiate(actor, layer, LoadedLevels[level.Iid]);
                                break;
                            case ICollider collider:
                                layer.Add(collider);
                                break;
                            case GenericLevelElement genericLevelElement:
                                layer.Add(genericLevelElement);
                                break;
                        }
                    }
                    break;
                case LDtkLayerType.Tiles:
                    // Get tileset.
                    // TODO: This nullable is probably bad lol
                    var set = Tilesets[ldtkLayer.TilesetDefUid ?? 0];
                    
                    // Instantiate each tile.
                    foreach (var tile in ldtkLayer.GridTiles)
                        LoadedLevels[level.Iid].AddTile(ldtkLayer.Identifier, level.WorldX + tile.Px[0], level.WorldY + tile.Px[1], new Tile(tile, set));
                    

                    break;
                case LDtkLayerType.AutoLayer:
                case LDtkLayerType.IntGrid:
                default:
                    throw new ArgumentOutOfRangeException();
            }
            // TODO: Handle entities, tiles, etc. etc.
        }
    }

    public static void InstantiateLevelByName(string name)
    {
        foreach (var level in ldtkLevels.Values)
        {
            if (level.Identifier == name)
                InstantiateLevel(level.Iid);
        }
    }

    public static void InstantiateAll()
    {
        foreach (var level in ldtkLevels.Values)
        {
            InstantiateLevel(level.Iid);
        }
    }

    public static void UninstantiateLevel(string iid)
    {
        if (inTick)
        {
            QueueLevelForUninstantiation(LoadedLevels[iid]);
            return;
        }
        
        if (ActiveLevels.ContainsKey(iid))
            ActiveLevels.Remove(iid);
        
        LoadedLevels.Remove(iid);
    }

    public static void UninstantiateLevelByName(string name)
    {
        foreach (var level in LoadedLevels.Values)
        {
            if (level.Name != name) 
                continue;
            
            LoadedLevels.Remove(level.Iid);
            return;
        }
    }

    public static void UninstantiateAll()
    {
        foreach (var level in LoadedLevels.Values)
        {
            UninstantiateLevel(level.Iid);
        }
    }

    public static void UninstantiateAllExcept(Level targetLevel)
    {
        foreach (var level in LoadedLevels.Values)
        {
            if (level != targetLevel)
                UninstantiateLevel(level.Iid);
        }
    }

    public static void ActivateLevel(string iid)
    {
        if (!LoadedLevels.ContainsKey(iid) || ActiveLevels.ContainsKey(iid))
            return;

        if (inTick)
        {
            QueueLevelForActivation(LoadedLevels[iid]);
            return;
        }

        ActiveLevels.Add(iid, LoadedLevels[iid]);
        LoadedLevels[iid].Active = true;
        
        // TODO: This doesn't take into account level resets yet.
        // (level resets don't exist at all as of writing)
        foreach (var actor in ActorManager.PersistentActors.Values)
        {
            if (actor.Layer == null)
                actor.OnLevelStart();
        }
        
        foreach (var layer in LoadedLevels[iid].Layers.Values)
        {
            foreach (var actor in layer.Actors)
            {
                actor.OnLevelStart();
            }
        }
    }
    
    public static void ActivateLevel(Level level)
    {
        ActivateLevel(level.Iid);
    }

    public static void ActivateLevelByName(string name)
    {
        foreach (var level in LoadedLevels.Values)
        {
            if (level.Name != name)
                continue;
            
            ActivateLevel(level.Iid);
            return;
        }
    }

    // TODO: Review hierarchy of overloads here.
    public static void DeactivateLevel(string iid)
    {
        if (!ActiveLevels.ContainsKey(iid))
            return;

        if (inTick)
        {
            QueueLevelForDeactivation(LoadedLevels[iid]);
            return;
        }

        ActiveLevels.Remove(iid);
        LoadedLevels[iid].Active = false;
    }

    public static void DeactivateLevel(Level level)
    {
        DeactivateLevel(level.Iid);
    }

    public static void DeactivateLevelByName(string name)
    {
        foreach (var level in ActiveLevels.Values)
        {
            if (level.Name == name)
                DeactivateLevel(level.Iid);
        }
    }

    public static Level GetLevelByName(string name)
    {
        return LoadedLevels.Values.FirstOrDefault(level => level.Name == name);
    }

    public static Level GetLevelByIid(string iid)
    {
        return LoadedLevels[iid];
    }

    public static void RenderLevels()
    {
        foreach (var level in ActiveLevels.Values)
        {
            level.Draw();
        }
    }

    private static void QueueLevelForDeactivation(Level level)
    {
        if (!levelsToBeDeactivated.Contains(level))
            levelsToBeDeactivated.Add(level);
    }
    
    private static void QueueLevelForActivation(Level level)
    {
        if (!levelsToBeActivated.Contains(level))
            levelsToBeActivated.Add(level);
    }
    
    private static void QueueLevelForInstantiation(LDtkLevelInstance level)
    {
        if (!levelsToBeInstantiated.Contains(level))
            levelsToBeInstantiated.Add(level);
    }
    
    private static void QueueLevelForUninstantiation(Level level)
    {
        if (!levelsToBeUninstantiated.Contains(level))
            levelsToBeUninstantiated.Add(level);
    }

    public static void Tick()
    {
        inTick = true;
        
        // TODO: Refactor this loop into each level individually?
        foreach (var level in ActiveLevels.Values)
        {
            foreach (var layer in level.Layers.Values)
            {
                layer.Tick();
            }
        }

        inTick = false;

        foreach (var level in levelsToBeDeactivated)
        {
            DeactivateLevel(level);
        }
        
        levelsToBeDeactivated.Clear();

        foreach (var level in levelsToBeUninstantiated)
        {
            UninstantiateLevel(level.Iid);
        }
        
        levelsToBeUninstantiated.Clear();

        foreach (var level in levelsToBeInstantiated)
        {
            InstantiateLevel(level.Iid);
        }
        
        levelsToBeInstantiated.Clear();

        foreach (var level in levelsToBeActivated)
        {
            ActivateLevel(level);
        }
        
        levelsToBeActivated.Clear();
    }
}