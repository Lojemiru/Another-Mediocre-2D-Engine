using Microsoft.Xna.Framework.Graphics;
using AM2E.Graphics;

namespace AM2E;

public interface IDrawable
{
    public void Draw(SpriteBatch spriteBatch);

    public int X { get; }
    public int Y { get; }
    public CullingBounds? CullingBounds { get; }
}