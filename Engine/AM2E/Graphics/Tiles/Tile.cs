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
    public readonly List<string>? EnumTags;
    
    private readonly Rectangle tileRect;
    private readonly SpriteEffects flips;
    private readonly int randomOffset = 0;
    
    private static readonly RNGInstance RNG = new();

    public Tile(LDtkTileInstance tile, Tileset tileset)
    {
        TilesetSprite = tileset.Sprite;
        tileRect = tileset.GetCachedTileRectangle(tile.Src[0] / tileset.GridSize, tile.Src[1] / tileset.GridSize);
        flips = (SpriteEffects)tile.F;
        randomOffset = RNG.Random(TilesetSprite.Length);
        EnumTags = tileset.GetEnumTags(tile.T);
    }

    public Tile(Sprite tilesetSprite, Rectangle tileBounds, byte flips)
    {
        TilesetSprite = tilesetSprite;
        tileRect = tileBounds;
        this.flips = (SpriteEffects)flips;
        randomOffset = RNG.Random(TilesetSprite.Length);
    }

    public void Draw(SpriteBatch spriteBatch, int frame, float x, float y, bool random = false, int layer = 0)
    {
        TilesetSprite.Draw(spriteBatch, x, y, frame + (random ? randomOffset : 0), tileRect, 0, flips, layer: layer);
    }

    public void Draw(SpriteBatch spriteBatch, int frame, float x, float y, float scale, bool random = false, int layer = 0)
    {
        TilesetSprite.Draw(spriteBatch, x, y, frame + (random ? randomOffset : 0), tileRect, 0, flips, scaleX:scale, scaleY:scale, layer: layer);
    }
}