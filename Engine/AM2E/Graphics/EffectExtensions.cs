using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DesktopBootstrapper.Graphics;

public static class EffectExtensions
{
    private static Vector2 stagingVector = new();
    public static void StageTextureSize(this Effect effect, Texture2D texture)
    {
        stagingVector.X = texture.Width;
        stagingVector.Y = texture.Height;
        effect.Parameters["TextureSize"].SetValue(stagingVector);
    }
}