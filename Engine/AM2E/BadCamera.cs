using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

namespace AM2E;

public static class BadCamera
{
    public static Matrix Matrix = Matrix.CreateScale(1, 1, 1);
    private static GraphicsDevice graphicsDevice;
    //private static Viewport viewport = new Viewport(0, 0, 426, 240);

    public static void Initialize(GraphicsDeviceManager graphicsDeviceManager)
    {
        graphicsDevice = graphicsDeviceManager.GraphicsDevice;
        //graphicsDevice.Viewport = viewport;
    }

    public static void Step()
    {
        //viewport.Bounds = new Rectangle(viewport.Bounds.X + 1, 0, 426, 240);
        //graphicsDevice.Viewport = viewport;

        Matrix.Translation = new(Matrix.Translation.X + 1, 0, 0);
    }
}