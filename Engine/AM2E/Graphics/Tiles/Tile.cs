using AM2E.Levels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Graphics;

public sealed class Tile
{
    public int Size => tileRect.Width;
    public int PosX => tileRect.X / tileRect.Width;
    public int PoxY => tileRect.Y / tileRect.Height;
    public readonly Sprite TilesetSprite;
    private readonly Rectangle tileRect;
    private readonly SpriteEffects flips;

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

    public void Draw(SpriteBatch spriteBatch, float x, float y, float scale)
    {
        TilesetSprite.Draw(spriteBatch, x, y, 0, tileRect, 0, flips, scaleX:scale, scaleY:scale);
    }
}