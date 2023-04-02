using System;
using AM2E.Levels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Graphics;

public sealed class Tile
{
    public int Size => tileRect.Width;
    private Sprite tilesetSprite;
    private Rectangle tileRect;
    private SpriteEffects flips;

    public Tile(LDtkTileInstance tile, Tileset tileset)
    {
        tilesetSprite = tileset.Sprite;
        tileRect = tileset.GetCachedTileRectangle(tile.Src[0] / tileset.GridSize, tile.Src[1] / tileset.GridSize);
        flips = (SpriteEffects)tile.F;
    }

    public Tile(Sprite tilesetSprite, Rectangle tileBounds, byte flips)
    {
        this.tilesetSprite = tilesetSprite;
        tileRect = tileBounds;
        this.flips = (SpriteEffects)flips;
    }

    public void Draw(SpriteBatch spriteBatch, int x, int y)
    {
        tilesetSprite.Draw(spriteBatch, x, y, 0, tileRect, 0, flips);
    }
}