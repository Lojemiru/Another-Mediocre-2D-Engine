using AM2E.Levels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Graphics;

public sealed class Tile : IDrawable
{
    public int X { get; }
    public int Y { get; }
    private Sprite tilesetSprite;
    private Rectangle tileRect;
    private SpriteEffects flips;

    public Tile(LDtkTileInstance tile, Tileset tileset, int x, int y)
    {
        X = x;
        Y = y;
        tilesetSprite = tileset.Sprite;
        tileRect = tileset.GetCachedTileRectangle(tile.Src[0] / tileset.GridSize, tile.Src[1] / tileset.GridSize);
        flips = (SpriteEffects)tile.F;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        tilesetSprite.Draw(spriteBatch, X, Y, 0, tileRect, 0, flips);
    }
}