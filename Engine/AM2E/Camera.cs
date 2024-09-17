using AM2E.Graphics;
using Microsoft.Xna.Framework;

namespace AM2E;

public static class Camera
{
    // I have NO CLUE how matrices work
    // ...but this guy does: https://www.youtube.com/watch?v=ceBCDKU_mNw
    public static int X { get; private set; } = 0;
    public static int Y { get; private set; } = 0;
    public static Matrix Transform { get; private set; }
    public static int Width => Renderer.GameWidth;
    public static int Height => Renderer.GameHeight;
    public static int BoundLeft => (int)X - Width / 2;
    public static int BoundRight => (int)X + Width / 2;
    public static int BoundTop => (int)Y - Height / 2;
    public static int BoundBottom => (int)Y + Height / 2;

    static Camera()
    {
        UpdateTransform();
    }

    public static void SetPosition(int x, int y)
    {
        X = x;
        Y = y;
        UpdateTransform();
    }

    public static void UpdateTransform()
    {
        // Center position translation
        Transform = Matrix.CreateTranslation(-X, -Y, 0) * 
                    // Adjust for current camera view width and height
                    // ReSharper disable twice PossibleLossOfFraction
                    Matrix.CreateTranslation(Renderer.ApplicationSurface.Width / (2 * Renderer.UpscaleAmount), Renderer.ApplicationSurface.Height / (2 * Renderer.UpscaleAmount), 0) *
                    // Adjust for upscale resolution
                    Matrix.CreateScale(Renderer.UpscaleAmount, Renderer.UpscaleAmount, 1);
    }
}