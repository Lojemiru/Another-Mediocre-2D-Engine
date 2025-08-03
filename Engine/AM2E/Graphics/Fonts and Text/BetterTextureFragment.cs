using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Graphics;

public class BetterTextureFragment : IRenderable
{
    public Vector2 Scale = Vector2.One;

    public Texture2D Texture { get; private set; }

    public Rectangle Region { get; private set; }

    public Point Size
    {
        get
        {
            return new Point((int) ((double) this.Region.Width * (double) this.Scale.X + 0.5), (int) ((double) this.Region.Height * (double) this.Scale.Y + 0.5));
        }
    }

    public BetterTextureFragment(Texture2D texture, Rectangle region)
    {
        this.Texture = texture != null ? texture : throw new ArgumentNullException(nameof (texture));
        this.Region = region;
    }

    public BetterTextureFragment(Texture2D texture)
        : this(texture, new Rectangle(0, 0, texture.Width, texture.Height))
    {
    }

    public void Draw(FSRenderContext context, Vector2 position, Color color)
    {
        context.DrawImage(this.Texture, this.Region, new Vector2(position.X, position.Y + 1), this.Scale, color);
    }
}