using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using AM2E.Actors;
using AM2E.Collision;
using AM2E.Graphics;
using GameContent;
using Newtonsoft.Json;

namespace AM2E.Levels;

public static class World
{
    private static LDtkWorldInstance world;
    private static LDtkLevelInstance[] levels;
    private static Dictionary<int, PageIndex> tilesetPageMappings = new();
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
            Enum.TryParse(entries[^2], out PageIndex index);
            tilesetPageMappings.Add(tileset.Uid, index);
        }
        
        levels = new LDtkLevelInstance[world.Levels.Length];
        
        // Load level data.
        var i = 0;
        foreach (var level in world.Levels)
        {
            using (var reader = File.OpenText("worlds/" + level.ExternalRelPath))
            {
                levels[i] = (LDtkLevelInstance)serializer.Deserialize(reader, typeof(LDtkLevelInstance));
            }
            ++i;
        }
    }

    public static void InstantiateLevel(int id)
    {
        // TODO: Review instantiation here for security
        
        var level = levels[id];
        
        foreach (var layer in level.LayerInstances)
        {
            // Handle collision first, since it's technically an Entities layer but we don't want to treat it as such
            if (layer.Identifier == "Collision")
            {
                foreach (var entity in layer.EntityInstances)
                {
                    var solidType = Type.GetType("GameContent." + entity.Identifier);
                    var solid = (ICollider)Activator.CreateInstance(solidType, entity, level.WorldX, level.WorldY);
                }
            }
            else
                switch (layer.Type)
                {
                    // TODO: asset layers :)
                    case LDtkLayerType.Entities:
                        foreach (var entity in layer.EntityInstances)
                        {
                            var entityType = Type.GetType("GameContent." + entity.Identifier);
                            var actor = (Actor)Activator.CreateInstance(entityType, entity, level.WorldX, level.WorldY);
                        }
                        break;
                    case LDtkLayerType.Tiles:
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
        for (var i = 0; i < levels.Length; i++)
        {
            InstantiateLevel(i);
        }
    }

    public static PageIndex GetTilesetPage(int uid)
    {
        // TODO: Throw if invalid uid is passed in.
        return tilesetPageMappings[uid];
    }
}