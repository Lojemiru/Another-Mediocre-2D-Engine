using System.Collections.Concurrent;
using AM2E.Graphics;
using Newtonsoft.Json;

namespace AM2E.Levels;

public static class World
{
    private static LDtkWorldInstance world;
    public static readonly Dictionary<string, LDtkLightweightLevelInstance> LdtkLevels = new();
    private static readonly ConcurrentDictionary<string, LDtkLevelInstance> StagedLevels = new();
    private static readonly ConcurrentDictionary<int, Tileset> Tilesets = new();
    private static readonly Dictionary<int, LDtkTilesetDefinition> LDtkTilesets = new();
    public static readonly Dictionary<int, LDtkCompositeBackgroundDefinition> LDtkBackgrounds = new();
    private static readonly Dictionary<string, Level> LoadedLevels = new();
    private static readonly ConcurrentQueue<Level> LoadingLevels = new();
    private static readonly Dictionary<string, Level> ActiveLevels = new();
    private static bool inTick = false;
    private static readonly List<Level> LevelsToBeActivated = new();
    private static readonly List<Level> LevelsToBeDeactivated = new();
    private static readonly List<Level> LevelsToBeUninstantiated = new();
    private static readonly ConcurrentDictionary<string, Action<Level>> StagedCallbacks = new();
    private static readonly Dictionary<string, Thread?> Threads = new();

    private static bool unloadQueued = false;
    private static Action? unloadCallback = null;

    private static readonly List<Action> OutOfTickCallbacks = new();

    private static string currentPath;

    public static int LevelUnitHeight => world.WorldGridHeight;
    public static int LevelUnitWidth => world.WorldGridWidth;

    public static void Unload(Action? callback = null)
    {
        unloadQueued = true;
        unloadCallback = callback;
    }

    private static void UnloadInternal(bool skipCallback = false)
    {
        // Yeah, this is kinda crappy... but it's the best I've got to forcibly shut down all current loads. I think.
        foreach (var thread in Threads)
            thread.Value?.Join();
        
        Threads.Clear();

        Tilesets.Clear();

        // If we've somehow loaded a level but haven't processed it, we want to make sure it is gracefully unloaded to prevent unexpected end user code explosions.
        foreach (var level in LoadedLevels.Values)
            UninstantiateLevel(level.Iid);

        LoadedLevels.Clear();

        // For any levels we just pulled out of the thread, we want to somewhat gracefully kill them.
        foreach (var _ in LoadingLevels)
        {
            LoadingLevels.TryDequeue(out var level);

            if (level is null) 
                continue;
            
            level.PostLoad();
            level.PreUnload();
            level.Dispose();
            level.PostUnload();
        }
        
        LoadingLevels.Clear();
        ActiveLevels.Clear();
        LevelsToBeActivated.Clear();
        LevelsToBeDeactivated.Clear();
        LdtkLevels.Clear();
        LDtkTilesets.Clear();
        LDtkBackgrounds.Clear();
        StagedLevels.Clear();
        LevelsToBeUninstantiated.Clear();
        StagedCallbacks.Clear();
        OutOfTickCallbacks.Clear();

        if (!skipCallback)
        {
            unloadCallback?.Invoke();
            unloadCallback = null;
        }

        unloadQueued = false;
    }

    public static void LoadWorld(string path, Action? callback = null)
    {
        UnloadInternal(true);

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
        
        // Load background definitions.
        foreach (var background in world.Defs.CompositeBackgrounds)
        {
            LDtkBackgrounds.Add(background.Uid, background);
        }
        
        currentPath = new FileInfo(path).Directory.FullName + "/";

        // Cache the IID of each lightweight level for faster grabbing later.
        foreach (var level in world.Levels)
        {
            LdtkLevels.Add(level.Iid, level);
            Threads.Add(level.Iid, null);
        }
    }

    private static void PopulateTiles(LDtkLevelInstance levelI, Level level, LDtkLayerInstance ldtkLayer)
    {
        var key = ldtkLayer.TilesetDefUid;
        
        if (key is null)
            throw new NullReferenceException(nameof(ldtkLayer.TilesetDefUid) + " is somehow null. This indicates a broken LDtk file.");

        if (Tilesets.ContainsKey((int)key))
        {
            PlaceTiles(levelI, level, ldtkLayer);
            return;
        }
        
        var tileset = LDtkTilesets[(int)key];
        var entries = tileset.RelPath.Split('/');
        var pageIndex = entries[^2];

        Action<TexturePage?> placeTiles = _ =>
        {
            if (!Tilesets.ContainsKey(tileset.Uid))
            {
                Tilesets.TryAdd(tileset.Uid,
                    new Tileset(TextureManager.GetSprite(pageIndex, tileset.Identifier), tileset));
            }

            PlaceTiles(levelI, level, ldtkLayer);
        };
        
        TextureManager.LoadPageBlocking(pageIndex);
        placeTiles(null);
    }

    private static void PlaceTiles(LDtkLevelInstance levelI, Level level, LDtkLayerInstance ldtkLayer)
    {
        var key = ldtkLayer.TilesetDefUid;

        if (key is null)
            throw new NullReferenceException(nameof(ldtkLayer.TilesetDefUid) + " is somehow null. This indicates a broken LDtk file.");
        
        if (ldtkLayer.Type == LDtkLayerType.Tiles)
            foreach (var tile in ldtkLayer.GridTiles)
                level.Add(ldtkLayer.Identifier, new Tile(tile, Tilesets[(int)key]), levelI.WorldX + tile.Px[0], levelI.WorldY + tile.Px[1]);
        else if (ldtkLayer.Type == LDtkLayerType.AutoLayer)
            foreach (var tile in ldtkLayer.AutoLayerTiles)
                level.Add(ldtkLayer.Identifier, new Tile(tile, Tilesets[(int)key]), levelI.WorldX + tile.Px[0], levelI.WorldY + tile.Px[1]);
    }

    private static void LoadLevelFromFile(LDtkLightweightLevelInstance lwLevel)
    {
        Logger.Engine($"Instantiating level {lwLevel.Identifier} ({lwLevel.Iid}) from file.");
        
        JsonSerializer serializer = new();
        using var reader = File.OpenText(currentPath + lwLevel.ExternalRelPath);
        var levelInstance = (LDtkLevelInstance)serializer.Deserialize(reader, typeof(LDtkLevelInstance));
        StagedLevels[lwLevel.Iid] = levelInstance;

        var id = lwLevel.Iid;
        
        Logger.Engine($"Instantiating level {LdtkLevels[id].Identifier} ({id}).");

        var level = new Level(levelInstance);

        level.PreLoad();
        
        foreach (var ldtkLayer in levelInstance.LayerInstances.Reverse())
        {
            // Create layer if it doesn't already exist.
            var layer = level.AddLayer(ldtkLayer.Identifier);

            switch (ldtkLayer.Type)
            {
                case LDtkLayerType.Entities:
                    foreach (var entity in ldtkLayer.EntityInstances)
                    {
                        var entityType = Type.GetType(EngineCore.ContentNamespaceHeader + "." + entity.Identifier + EngineCore.ContentNamespaceFooter);

                        if (entityType is null)
                        {
                            Logger.Warn($"Unable to instantiate entity {entity.Iid} in level {levelInstance.Iid}: " +
                                        $"Class {entity.Identifier} does not exist in {EngineCore.ContentNamespaceHeader}!");
                        }
                        else
                        {
                            Activator.CreateInstance(entityType, entity, levelInstance.WorldX + entity.Px[0], 
                                levelInstance.WorldY + entity.Px[1], layer);
                        }
                    }
                    break;
                case LDtkLayerType.Tiles:
                case LDtkLayerType.AutoLayer:
                    PopulateTiles(levelInstance, level, ldtkLayer);
                    break;
                case LDtkLayerType.IntGrid:
                    // Do nothing.
                    // We don't want to crash on these because they're needed for AutoLayers, but I have no other use for them.
                    // Yet.
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        level.AsyncPostLoad();

        LoadingLevels.Enqueue(level);
    }

    /// <summary>
    /// Instantiates the level that corresponds to the supplied ID.
    /// </summary>
    /// <param name="id">The ID of the level to instantiate.</param>
    /// <param name="callback">A callback to run when instantiation is finished.
    /// <br></br>WARNING: If multiple load calls are used, only the first defined callback will be used!</param>
    /// <param name="blocking">Whether to instantiate the level in a blocking fashion rather than asynchronously.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void InstantiateLevel(string id, Action<Level>? callback = null, bool blocking = false)
    {
        if (LoadedLevels.TryGetValue(id, out var value))
        {
            callback?.Invoke(value);
            return;
        }

        if (inTick || !StagedLevels.ContainsKey(id))
        {
            if (callback is not null)
                StagedCallbacks.TryAdd(id, callback);
            
            if (!blocking)
            {
                if (Threads[id] != null)
                    return;
                
                Logger.Engine($"Initiating background load for level {LdtkLevels[id].Identifier} ({id})");
                Threads[id] = new Thread(() => LoadLevelFromFile(LdtkLevels[id]))
                {
                    IsBackground = true
                };
                Threads[id]!.Start();
            }
            else
            {
                if (Threads[id] != null)
                {
                    Logger.Engine($"Joining existing instantiation thread for level {LdtkLevels[id].Identifier} ({id})");
                    Threads[id]!.Join();
                }
                else
                {
                    Logger.Engine($"Initiating blocking load for level {LdtkLevels[id].Identifier} ({id})");
                    LoadLevelFromFile(LdtkLevels[id]);
                }
            }
        }
    }

    public static void InstantiateLevelByName(string name, Action<Level>? callback = null, bool blocking = false)
    {
        foreach (var level in LdtkLevels.Values)
        {
            if (level.Identifier != name) 
                continue;
            
            InstantiateLevel(level.Iid, callback, blocking);
            return;
        }
    }

    public static void InstantiateAll()
    {
        foreach (var level in LdtkLevels.Values)
            InstantiateLevel(level.Iid);
    }

    public static void UninstantiateLevel(string iid) 
        => UninstantiateLevel(iid, false);
    
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
    }

    public static void UninstantiateAllExcept(params Level[] targetLevels)
    {
        foreach (var level in LoadedLevels.Values)
        {
            if (!targetLevels.Contains(level))
                UninstantiateLevel(level.Iid, false);
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

    public static Level? GetLevelByName(string name)
    {
        return LoadedLevels.Values.FirstOrDefault(level => level.Name == name);
    }

    public static Level GetLevelByIid(string iid)
    {
        if (!IsLevelLoaded(iid))
            throw new KeyNotFoundException($"Level {iid} is not loaded!");
        
        return LoadedLevels[iid];
    }

    public static bool IsLevelLoaded(string iid)
    {
        return LoadedLevels.ContainsKey(iid);
    }

    public static LDtkCompositeBackgroundDefinition GetCompositeBackground(int uid)
    {
        return LDtkBackgrounds[uid];
    }

    public static void RenderLevels()
    {
        foreach (var level in ActiveLevels.Values)
            level.Draw();
    }

    private static void QueueLevelForDeactivation(Level level)
    {
        if (!LevelsToBeDeactivated.Contains(level))
            LevelsToBeDeactivated.Add(level);
    }
    
    private static void QueueLevelForActivation(Level level)
    {
        if (!LevelsToBeActivated.Contains(level))
            LevelsToBeActivated.Add(level);
    }

    private static void QueueLevelForUninstantiation(Level level)
    {
        if (!LevelsToBeUninstantiated.Contains(level))
            LevelsToBeUninstantiated.Add(level);
    }

    internal static void PreTick()
    {
        inTick = true;
        foreach (var level in ActiveLevels.Values)
            level.PreTick();
    }

    internal static void Tick()
    {
        foreach (var level in ActiveLevels.Values)
            level.Tick();
    }

    internal static void PostTick()
    {
        foreach (var level in ActiveLevels.Values)
            level.PostTick();
        
        inTick = false;
        
        foreach (var action in OutOfTickCallbacks)
            action.Invoke();
        
        OutOfTickCallbacks.Clear();

        foreach (var level in LevelsToBeDeactivated)
            DeactivateLevel(level);
        
        LevelsToBeDeactivated.Clear();

        foreach (var level in LevelsToBeUninstantiated)
            UninstantiateLevel(level.Iid);

        LevelsToBeUninstantiated.Clear();

        foreach (var l in LoadingLevels)
        {
            if (!LoadingLevels.TryDequeue(out var level))
                Logger.Warn($"Engine warning: level instantiation dequeue failed for {l.Iid}! Expecting catastrophic failure..." );
            
            var id = level!.Iid;

            LoadedLevels.Add(id, level);
            level.PostLoad();

            StagedLevels.TryRemove(id, out _);
            if (StagedCallbacks.TryGetValue(id, out var finalCallback))
            {
                finalCallback?.Invoke(level);
                StagedCallbacks.TryRemove(id, out _);
            }
            
            Threads[level.Iid] = null;
        }

        foreach (var level in LevelsToBeActivated)
            ActivateLevel(level);

        LevelsToBeActivated.Clear();

        if (unloadQueued)
            UnloadInternal();
    }

    public static LDtkReferenceToAnEntityInstance GetFirstFromToC(string identifier)
    {
        foreach (var entry in world.TableOfContent)
        {
            if (entry.Identifier == identifier && entry.Instances.Length > 0)
                return entry.Instances[0];
        }
        
        return default;
    }

    public static IEnumerable<LDtkReferenceToAnEntityInstance>? GetAllFromToC(string identifier)
    {
        foreach (var entry in world.TableOfContent)
        {
            if (entry.Identifier == identifier)
                return entry.Instances;
        }

        return null;
    }

    public static void DoAfterThisTick(Action action)
    {
        OutOfTickCallbacks.Add(action);
    }
}