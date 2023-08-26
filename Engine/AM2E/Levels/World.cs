using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AM2E.Graphics;
using GameContent;
using Newtonsoft.Json;

namespace AM2E.Levels;

public static class World
{
    private static LDtkWorldInstance world;
    private static readonly Dictionary<string, LDtkLightweightLevelInstance> ldtkLevels = new();
    private static readonly ConcurrentDictionary<string, LDtkLevelInstance> stagedLevels = new();
    private static readonly Dictionary<int, Tileset> Tilesets = new();
    private static readonly Dictionary<int, LDtkTilesetDefinition> LDtkTilesets = new();
    public static Dictionary<string, Level> LoadedLevels = new();
    public static Dictionary<string, Level> ActiveLevels = new();
    private static bool inTick = false;
    private static List<Level> levelsToBeActivated = new();
    private static List<Level> levelsToBeDeactivated = new();
    private static readonly List<LDtkLevelInstance> levelsToBeInstantiated = new();
    private static readonly List<LDtkLevelInstance> deferredLevelsToBeInstantiated = new();
    private static bool inInstantiation = false;
    private static readonly List<Level> levelsToBeUninstantiated = new();
    private static readonly ConcurrentDictionary<string, Action> stagedCallbacks = new();

    private static string currentPath;

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
        stagedLevels.Clear();
        deferredLevelsToBeInstantiated.Clear();
        
        JsonSerializer serializer = new();
        using (var reader = File.OpenText(path))
        {
            world = (LDtkWorldInstance)serializer.Deserialize(reader, typeof(LDtkWorldInstance));
        }

        // Load tileset definitions.
        foreach (var tileset in world.Defs.Tilesets)
        {
            LDtkTilesets.Add(tileset.Uid, tileset);
        }
        
        currentPath = new FileInfo(path).Directory.FullName + "/";

        // Cache the IID of each lightweight level for faster grabbing later.
        foreach (var level in world.Levels)
        {
            ldtkLevels.Add(level.Iid, level);
        }
    }

    private static void PopulateTiles(LDtkLevelInstance level, LDtkLayerInstance ldtkLayer)
    {
        var key = ldtkLayer.TilesetDefUid;
        
        if (key is null)
            throw new NullReferenceException(nameof(ldtkLayer.TilesetDefUid) + " is somehow null. This indicates a broken LDtk file.");

        if (Tilesets.ContainsKey((int)key))
        {
            PlaceTiles(level, ldtkLayer);
            return;
        }
        
        var tileset = LDtkTilesets[(int)key];
        var entries = tileset.RelPath.Split('/');
        Enum.TryParse(entries[^2], out PageIndex pageIndex);
        Enum.TryParse(tileset.Identifier, out SpriteIndex spriteIndex);
        TextureManager.LoadPage(pageIndex, _ =>
        {
            Tilesets.Add(tileset.Uid, new Tileset(TextureManager.GetSprite(pageIndex, spriteIndex), tileset));
            PlaceTiles(level, ldtkLayer);
        });
    }

    private static void PlaceTiles(LDtkLevelInstance level, LDtkLayerInstance ldtkLayer)
    {
        var key = ldtkLayer.TilesetDefUid;

        if (key is null)
            throw new NullReferenceException(nameof(ldtkLayer.TilesetDefUid) + " is somehow null. This indicates a broken LDtk file.");
        
        foreach (var tile in ldtkLayer.GridTiles)
            LoadedLevels[level.Iid].Add(ldtkLayer.Identifier, new Tile(tile, Tilesets[(int)key]), level.WorldX + tile.Px[0], level.WorldY + tile.Px[1]);
    }

    private static void LoadLevelFromFile(LDtkLightweightLevelInstance level)
    {
        JsonSerializer serializer = new();
        using var reader = File.OpenText(currentPath + level.ExternalRelPath);
        var levelInstance = (LDtkLevelInstance)serializer.Deserialize(reader, typeof(LDtkLevelInstance));
        stagedLevels[level.Iid] = levelInstance;
        QueueLevelForInstantiation(levelInstance);
    }

    public static void InstantiateLevel(string id, Action callback = null)
    {
        if (LoadedLevels.ContainsKey(id))
            return;

        if (inTick || !stagedLevels.ContainsKey(id))
        {
            stagedCallbacks.TryAdd(id, callback);
            
            var t = new Thread(() => LoadLevelFromFile(ldtkLevels[id]))
            {
                IsBackground = true
            };
            t.Start();

            return;
        }

        var level = stagedLevels[id];
        
        LoadedLevels.Add(level.Iid, new Level(level));
        
        LoadedLevels[level.Iid].PreLoad();
        
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
                        var entityType = Type.GetType(EngineCore.ContentNamespace + "." + entity.Identifier);
                        Activator.CreateInstance(entityType, entity, level.WorldX + entity.Px[0], level.WorldY + entity.Px[1], layer);
                    }
                    break;
                case LDtkLayerType.Tiles:
                    // Get tileset.
                    PopulateTiles(level, ldtkLayer);

                    break;
                case LDtkLayerType.AutoLayer:
                case LDtkLayerType.IntGrid:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        LoadedLevels[level.Iid].PostLoad();

        stagedLevels.TryRemove(id, out _);
        stagedCallbacks[id]?.Invoke();

        stagedCallbacks.TryRemove(id, out _);
    }

    public static void InstantiateLevelByName(string name, Action callback = null)
    {
        foreach (var level in ldtkLevels.Values)
        {
            if (level.Identifier != name) 
                continue;
            
            InstantiateLevel(level.Iid, callback);
            return;
        }
    }

    public static void InstantiateAll()
    {
        foreach (var level in ldtkLevels.Values)
            InstantiateLevel(level.Iid);
    }

    public static void UninstantiateLevel(string iid) => UninstantiateLevel(iid, true);
    
    private static void UninstantiateLevel(string iid, bool collect)
    {
        if (inTick)
        {
            QueueLevelForUninstantiation(LoadedLevels[iid]);
            return;
        }

        LoadedLevels[iid].PreUnload();
        
        if (ActiveLevels.ContainsKey(iid))
            ActiveLevels.Remove(iid);

        LoadedLevels[iid].Dispose();
        
        LoadedLevels[iid].PostUnload();

        LoadedLevels.Remove(iid);

        if (collect)
            GC.Collect();
    }
    
    public static void UninstantiateLevelByName(string name)
    {
        foreach (var level in LoadedLevels.Values)
        {
            if (level.Name != name) 
                continue;
            
            UninstantiateLevel(level.Iid);
            return;
        }
    }

    public static void UninstantiateAll()
    {
        foreach (var level in LoadedLevels.Values)
            UninstantiateLevel(level.Iid, false);
        
        GC.Collect();
    }

    public static void UninstantiateAllExcept(Level targetLevel)
    {
        foreach (var level in LoadedLevels.Values)
        {
            if (level != targetLevel)
                UninstantiateLevel(level.Iid, false);
        }
        
        GC.Collect();
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
        LoadedLevels[iid].Activate();
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
        LoadedLevels[iid].Deactivate();
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
            level.Draw();
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
        {
            if (!inInstantiation)
                levelsToBeInstantiated.Add(level);
            else
                deferredLevelsToBeInstantiated.Add(level);
        }
    }
    
    private static void QueueLevelForUninstantiation(Level level)
    {
        if (!levelsToBeUninstantiated.Contains(level))
            levelsToBeUninstantiated.Add(level);
    }

    internal static void PreTick(bool isFastForward)
    {
        inTick = true;
        foreach (var level in ActiveLevels.Values)
            level.PreTick(isFastForward);
    }

    internal static void Tick(bool isFastForward)
    {
        foreach (var level in ActiveLevels.Values)
            level.Tick(isFastForward);
    }

    internal static void PostTick(bool isFastForward)
    {
        foreach (var level in ActiveLevels.Values)
            level.PostTick(isFastForward);
        
        inTick = false;

        foreach (var level in levelsToBeDeactivated)
            DeactivateLevel(level);
        
        levelsToBeDeactivated.Clear();

        foreach (var level in levelsToBeUninstantiated)
            UninstantiateLevel(level.Iid);

        levelsToBeUninstantiated.Clear();

        inInstantiation = true;
        
        foreach (var level in levelsToBeInstantiated)
            InstantiateLevel(level.Iid);

        levelsToBeInstantiated.Clear();

        inInstantiation = false;

        // ...yes, this is kind of crappy. But it fixed my multithreaded loading problems, so... cope? Or show me a better solution lol
        foreach (var level in deferredLevelsToBeInstantiated)
            InstantiateLevel(level.Iid);

        deferredLevelsToBeInstantiated.Clear();

        foreach (var level in levelsToBeActivated)
            ActivateLevel(level);

        levelsToBeActivated.Clear();
    }

    public static LDtkReferenceToAnEntityInstance GetFirstFromToC(string objectName)
    {
        foreach (var entry in world.TableOfContent)
        {
            if (entry.Identifier == objectName)
                return entry.Instances[0];
        }
        
        return default;
    }
}