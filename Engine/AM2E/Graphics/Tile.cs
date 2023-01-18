using AM2E.Levels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Graphics;

public class Tile : IDrawable
{
    public int X { get; }
    public int Y { get; }
    private Sprite tilesetSprite;
    private Rectangle tileRect;
    private SpriteEffects flips;

    public Tile(LDtkTileInstance tile, Sprite tileset, int x, int y, int size)
    {
        X = x;
        Y = y;
        tilesetSprite = tileset;
        // TODO: Index tileset tile rectangles somehow and load that into this constructor instead. Should be a lot less memory intensive.
        tileRect = new Rectangle(tile.Src[0], tile.Src[1], size, size);
        flips = (SpriteEffects)tile.F;
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        tilesetSprite.Draw(spriteBatch, X, Y, 0, tileRect, 0, flips);
    }
}