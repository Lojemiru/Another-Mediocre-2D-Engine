using AM2E.Levels;
using Microsoft.Xna.Framework;

namespace AM2E.Graphics;

public sealed class Tileset
{
    public readonly Sprite Sprite;
    private readonly Rectangle[,] tileCache;
    private readonly Dictionary<int, List<string>> enumTags = new();
    public readonly int GridSize;

    public Tileset(Sprite sprite, LDtkTilesetDefinition definition)
    {
        Sprite = sprite;
        tileCache = new Rectangle[definition.CWid, definition.CHei];
        GridSize = definition.TileGridSize;
        
        foreach (var tag in definition.EnumTags)
        {
            foreach (var id in tag.TileIds)
            {
                if (!enumTags.TryGetValue(id, out var val))
                    enumTags.Add(id, [ tag.EnumValueId ]);
                else if (!val.Contains(tag.EnumValueId))
                        val.Add(tag.EnumValueId);
            }
        }
    }

    public Rectangle GetCachedTileRectangle(int x, int y)
    {
        if (tileCache[x, y] == Rectangle.Empty)
            tileCache[x, y] = new Rectangle(x * GridSize, y * GridSize, GridSize, GridSize);

        return tileCache[x, y];
    }

    public List<string>? GetEnumTags(int tileId)
    {
        return enumTags.GetValueOrDefault(tileId);
    }
}