using AM2E.Levels;
using Microsoft.Xna.Framework;

namespace AM2E.Graphics;

public sealed class Tileset
{
    public readonly Sprite Sprite;
    private readonly Rectangle[,] tileCache;
    public readonly int GridSize;

    public Tileset(Sprite sprite, LDtkTilesetDefinition definition)
    {
        Sprite = sprite;
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