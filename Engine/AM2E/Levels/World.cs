using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    private static LDtkLevelInstance[] ldtkLevels;
    private static Dictionary<int, LDtkTilesetDefinition> tilesets = new();
    private static Dictionary<int, PageIndex> tilesetPageMappings = new();
    private static Dictionary<int, Level> loadedLevels = new();
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

            tilesets.Add(tileset.Uid, tileset);
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

        var depth = 500;
        
        // TODO: Throw if already exists
        loadedLevels.Add(id, new Level(level.Identifier, level.WorldX, level.WorldY, level.PxWid, level.PxHei));
        
        foreach (var ldtkLayer in level.LayerInstances)
        {
            Console.WriteLine(ldtkLayer.Identifier);
            Console.WriteLine(depth);
            // Create layer if it doesn't already exist.
            var layer = loadedLevels[id].AddLayer(ldtkLayer.Identifier, depth);
            
            // Handle collision first, since it's technically an Entities layer but we don't want to treat it as such
            // TODO: We need to have a means of loading multiple collision layers.
            // TODO: We may need to have a means of intentionally choosing *not* to instantiate a layer.
            if (ldtkLayer.Identifier == "Collision")
            {
                // TODO: Assign to layers
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
                            var actor = (Actor)Activator.CreateInstance(entityType, entity, level.WorldX, level.WorldY);
                            layer.Add(actor);
                        }
                        break;
                    case LDtkLayerType.Tiles:
                        // Get tileset sprite and other metadata.
                        // TODO: This nullable is probably bad lol
                        var set = tilesets[ldtkLayer.TilesetDefUid ?? 0];
                        Enum.TryParse(set.Identifier, out SpriteIndex index);
                        var sprite = TextureManager.GetPage(GetTilesetPage(set.Uid)).Sprites[index];
                        
                        // Instantiate each tile.
                        foreach (var tile in ldtkLayer.GridTiles)
                            loadedLevels[id].AddDrawable(ldtkLayer.Identifier, new Tile(tile, sprite, level.WorldX + tile.Px[0], level.WorldY + tile.Px[1], set.TileGridSize));
                        
                        break;
                    case LDtkLayerType.AutoLayer:
                    case LDtkLayerType.IntGrid:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            // TODO: Handle entities, tiles, etc. etc.
            depth -= 100;
        }
        
    }

    public static void InstantiateAll()
    {
        for (var i = 0; i < ldtkLevels.Length; i++)
        {
            InstantiateLevel(i);
        }
    }

    public static PageIndex GetTilesetPage(int uid)
    {
        // TODO: Throw if invalid uid is passed in.
        return tilesetPageMappings[uid];
    }

    public static Level GetLevel(string name)
    {
        return loadedLevels.Values.FirstOrDefault(level => level.Name == name);
    }

    public static void RenderLevels()
    {
        // TODO: switch drawing based on whether or not level is flagged as active... or have two variables? One for visibility, one for activeness?
        foreach (var level in loadedLevels.Values)
        {
            level.Draw();
        }
    }
}