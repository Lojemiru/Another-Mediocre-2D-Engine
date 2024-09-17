using AM2E.Levels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Graphics;

public sealed class Tile
{
    public int Size => tileRect.Width;
    public readonly Sprite TilesetSprite;
    private Rectangle tileRect;
    private SpriteEffects flips;

    public Tile(LDtkTileInstance tile, Tileset tileset)
    {
        TilesetSprite = tileset.Sprite;
        tileRect = tileset.GetCachedTileRectangle(tile.Src[0] / tileset.GridSize, tile.Src[1] / tileset.GridSize);
        flips = (SpriteEffects)tile.F;
    }

    public Tile(Sprite tilesetSprite, Rectangle tileBounds, byte flips)
    {
        TilesetSprite = tilesetSprite;
        tileRect = tileBounds;
        this.flips = (SpriteEffects)flips;
    }

    public void Draw(SpriteBatch spriteBatch, float x, float y)
    {
        TilesetSprite.Draw(spriteBatch, x, y, 0, tileRect, 0, flips);
    }
}