using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Graphics;

public class BetterTextureFragment(Texture2D texture, Rectangle region) : IRenderable
{
    public Vector2 Scale = Vector2.One;

    public Texture2D Texture { get; } = texture != null ? texture : throw new ArgumentNullException(nameof (texture));

    public Rectangle Region { get; } = region;

    public Point Size => new((int) (Region.Width * (double) Scale.X + 0.5), (int) (Region.Height * (double) Scale.Y + 0.5));

    public BetterTextureFragment(Texture2D texture)
        : this(texture, new Rectangle(0, 0, texture.Width, texture.Height))
    { }

    public void Draw(FSRenderContext context, Vector2 position, Color color)
    {
        context.DrawImage(Texture, Region, new Vector2(position.X, position.Y + 1), Scale, color);
    }
}