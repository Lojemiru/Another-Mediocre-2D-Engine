using System;
using AM2E.Actors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

namespace AM2E;

public static class GameCamera
{
    // I have NO CLUE how matrices work
    // ...but this guy does: https://www.youtube.com/watch?v=ceBCDKU_mNw
    public static int Width { get; private set; } = 1920;
    public static int Height { get; private set; } = 1080;
    public static int X { get; private set; } = 0;
    public static int Y { get; private set; } = 0;
    public static Matrix Transform { get; private set; }

    static GameCamera()
    {
        UpdateTransform();
    }
    
    public static void SetPosition(int x, int y)
    {
        X = x;
        Y = y;
        UpdateTransform();
    }

    public static void SetSize(int width, int height)
    {
        Width = width;
        Height = height;
        UpdateTransform();
    }

    private static void UpdateTransform()
    {
        Transform = Matrix.CreateTranslation(-X, -Y, 0) * Matrix.CreateTranslation(Width / 2, Height / 2, 0);
    }
}