using System.Collections.Generic;
using AM2E.Graphics;

namespace AM2E.Levels;

public class Level
{
    public readonly string Name;
    public readonly int X;
    public readonly int Y;
    public readonly int Width;
    public readonly int Height;
    private readonly Dictionary<string, Layer> layers = new();

    public Level(string name, int x, int y, int width, int height)
    {
        Name = name;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
    
    public void AddLayer(string name, int depth)
    {
        // TODO: throw if layer already exists
        layers.Add(name, new Layer(name, depth));
    }

    public Layer GetLayer(string name)
    {
        // TODO: Throw if name doesn't exist
        return layers[name];
    }

    public void AddDrawable(string layerName, IDrawable drawable)
    {
        // TODO: Throw if name doesn't exist
        layers[layerName].Add(drawable);
    }

    public void Draw()
    {
        foreach (var layer in layers.Values)
        {
            layer.Draw();
        }
    }
}