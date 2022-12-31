using System;
using System.IO;
using System.Reflection;
using AM2E.Collision;
using GameContent;
using Newtonsoft.Json;

namespace AM2E.Levels;

public static class World
{
    private static LDtkWorldInstance world;
    private static LDtkLevelInstance[] levels;
    public static void LoadWorld(string path)
    {
        JsonSerializer serializer = new();
        using (var reader = File.OpenText(path))
        {
            world = (LDtkWorldInstance)serializer.Deserialize(reader, typeof(LDtkWorldInstance));
        }

        levels = new LDtkLevelInstance[world.Levels.Length];
        
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
            if (layer.Identifier == "Collision")
            {
                foreach (var entity in layer.EntityInstances)
                {
                    Type solidType = Type.GetType("GameContent." + entity.Identifier);
                    ICollider solid = (ICollider)Activator.CreateInstance(solidType, entity, level.WorldX, level.WorldY);
                }
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
}