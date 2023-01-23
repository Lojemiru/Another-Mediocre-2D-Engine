using System;
using System.Collections.Generic;
using System.Linq;
using AM2E.Actors;
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
    public bool Active = false;
    public bool Visible = true; // TODO: default to false

    public Level(string name, int x, int y, int width, int height)
    {
        Name = name;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
    
    public Layer AddLayer(string name, int depth)
    {
        if (layers.ContainsKey(name))
            throw new ArgumentException("A layer with the specified name \"" + name + "\" already exists in level \"" + Name + "\"");
        
        layers.Add(name, new Layer(name, depth));

        return layers[name];
    }

    public Layer GetLayer(string name)
    {
        try
        {
            return layers[name];
        }
        catch
        {
            return null;
        }
    }

    public void AddDrawable(string layerName, IDrawable drawable)
    {
        if (!layers.ContainsKey(layerName))
            throw new ArgumentException("No layer with the specified name \"" + layerName + "\" exists in level \"" + Name + "\"");
        
        layers[layerName].Add(drawable);
    }

    public void Add(string layerName, IDrawable drawable)
    {
        layers[layerName].Add(drawable);
    }
    
    public void Add(string layerName, Actor actor)
    {
        layers[layerName].Add(actor);
    }
    
    public void Add(string layerName, object obj)
    {
        layers[layerName].Add(obj);
    }

    public void Draw()
    {
        if (!Visible) return;
        foreach (var layer in layers.Values)
        {
            layer.Draw();
        }
    }
}