using AM2E.Levels;
using Microsoft.Xna.Framework;

namespace AM2E.Graphics;

public sealed class Tileset
{
    private readonly Rectangle[,] tileCache;
    public readonly int GridSize;
    public readonly string Index;
    public readonly string Key;

    public Tileset(string index, string key, LDtkTilesetDefinition definition)
    {
        Index = index;
        Key = key;
        tileCache = new Rectangle[definition.CWid, definition.CHei];
        GridSize = definition.TileGridSize;
    }

    public Rectangle GetCachedTileRectangle(int x, int y)
    {
        if (tileCache[x, y] == Rectangle.Empty)
            tileCache[x, y] = new Rectangle(x * GridSize, y * GridSize, GridSize, GridSize);

        return tileCache[x, y];
    }
}