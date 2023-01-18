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
    private static Dictionary<int, LDtkTilesetDefinition> tilesets = new();
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

            tilesets.Add(tileset.Uid, tileset);
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

        var depth = 500;
        
        foreach (var layer in level.LayerInstances)
        {
            // Handle collision first, since it's technically an Entities layer but we don't want to treat it as such
            // TODO: We need to have a means of loading multiple collision layers.
            // TODO: We may need to have a means of intentionally choosing *not* to instantiate a layer.
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
                        // Create layer if it doesn't already exist.
                        // TODO: AHHHHH LAYERS ARE UNIVERSAL INSTEAD OF PER-ROOM MAKE THEM PER-ROOM
                        if (Renderer.GetLayer(layer.Identifier) == null) Renderer.AddLayer(layer.Identifier, depth);
                        
                        // Get tileset sprite and other metadata.
                        // TODO: This nullable is probably bad lol
                        var set = tilesets[layer.TilesetDefUid ?? 0];
                        Enum.TryParse(set.Identifier, out SpriteIndex index);
                        var sprite = TextureManager.GetPage(GetTilesetPage(set.Uid)).Sprites[index];
                        
                        // Instantiate each tile.
                        foreach (var tile in layer.GridTiles)
                            Renderer.AddDrawable(layer.Identifier, new Tile(tile, sprite, level.WorldX + tile.Px[0], level.WorldY + tile.Px[1], set.TileGridSize));
                        
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