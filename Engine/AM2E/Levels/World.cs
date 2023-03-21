using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AM2E.Actors;
using AM2E.Collision;
using AM2E.Graphics;
using GameContent;
using Newtonsoft.Json;

namespace AM2E.Levels;

// TODO: Generic ILevelElement interface for collision/triggers/etc. - things that aren't Actors but need to retain a link to their Layer, Level, etc.
// TODO: Use a similar pattern and the is keyword to instantiate level elements and Actors without relying on layer names in LDtk.

public static class World
{
    private static LDtkWorldInstance world;
    private static LDtkLevelInstance[] ldtkLevels;
    private static readonly Dictionary<int, Tileset> Tilesets = new();
    public static Dictionary<string, Level> LoadedLevels = new();
    public static Dictionary<string, Level> ActiveLevels = new();

    public static int LevelUnitHeight => world.WorldGridHeight;
    public static int LevelUnitWidth => world.WorldGridWidth;

    public static void LoadWorld(string path)
    {
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
        
        ldtkLevels = new LDtkLevelInstance[world.Levels.Length];
        
        // Load level data.
        var i = 0;
        foreach (var level in world.Levels)
        {
            using (var reader = File.OpenText("worlds/" + level.ExternalRelPath))
            {
                ldtkLevels[i] = (LDtkLevelInstance)serializer.Deserialize(reader, typeof(LDtkLevelInstance));
            }
            ++i;
        }
    }

    public static void InstantiateLevel(int id)
    {
        // TODO: Review instantiation here for security
        
        var level = ldtkLevels[id];

        if (LoadedLevels.ContainsKey(level.Iid))
            throw new Exception("Key " + level.Iid + " has already been instantiated!");
        
        LoadedLevels.Add(level.Iid, new Level(level));
        
        foreach (var ldtkLayer in level.LayerInstances.Reverse())
        {
            // Create layer if it doesn't already exist.
            var layer = LoadedLevels[level.Iid].AddLayer(ldtkLayer.Identifier);
            
            // Handle collision first, since it's technically an Entities layer but we don't want to treat it as such
            // TODO: We need to have a means of loading multiple collision layers.
            // TODO: We may need to have a means of intentionally choosing *not* to instantiate a layer.
            if (ldtkLayer.Identifier == "Collision")
            {
                foreach (var entity in ldtkLayer.EntityInstances)
                {
                    var solidType = Type.GetType("GameContent." + entity.Identifier);
                    var solid = (ICollider)Activator.CreateInstance(solidType, entity, level.WorldX, level.WorldY);
                    layer.Add(solid);
                }
            }
            else
                switch (ldtkLayer.Type)
                {
                    // TODO: asset layers :)
                    case LDtkLayerType.Entities:
                        foreach (var entity in ldtkLayer.EntityInstances)
                        {
                            var entityType = Type.GetType("GameContent." + entity.Identifier);
                            var ent = (Actor)Activator.CreateInstance(entityType, entity, level.WorldX + entity.Px[0], level.WorldY + entity.Px[1]);
                            ActorManager.Instantiate(ent, layer, LoadedLevels[level.Iid]);
                        }
                        break;
                    case LDtkLayerType.Tiles:
                        // Get tileset.
                        // TODO: This nullable is probably bad lol
                        var set = Tilesets[ldtkLayer.TilesetDefUid ?? 0];
                        
                        // Instantiate each tile.
                        foreach (var tile in ldtkLayer.GridTiles)
                            LoadedLevels[level.Iid].AddDrawable(ldtkLayer.Identifier, new Tile(tile, set, level.WorldX + tile.Px[0], level.WorldY + tile.Px[1]));
                        

                        break;
                    case LDtkLayerType.AutoLayer:
                    case LDtkLayerType.IntGrid:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            // TODO: Handle entities, tiles, etc. etc.
        }
        
    }

    public static void InstantiateAll()
    {
        for (var i = 0; i < ldtkLevels.Length; i++)
        {
            InstantiateLevel(i);
        }
    }

    public static void ActivateLevel(string iid)
    {
        if (!LoadedLevels.ContainsKey(iid) || ActiveLevels.ContainsKey(iid))
            return;

        ActiveLevels.Add(iid, LoadedLevels[iid]);
    }
    
    public static void ActivateLevel(Level level)
    {
        ActivateLevel(level.Iid);
    }

    public static void ActivateLevelByName(string name)
    {
        foreach (var level in LoadedLevels.Values)
        {
            if (level.Name == name)
                ActivateLevel(level);
        }
    }

    public static Level GetLevel(string name)
    {
        return LoadedLevels.Values.FirstOrDefault(level => level.Name == name);
    }

    public static void RenderLevels()
    {
        foreach (var level in ActiveLevels.Values)
        {
            level.Draw();
        }
    }
}