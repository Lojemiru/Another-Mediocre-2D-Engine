using System;
using System.Collections.Generic;
using AM2E.Actors;
using AM2E.Graphics;

namespace AM2E.Levels;

public sealed class Level
{
    public readonly string Name;
    public readonly int X;
    public readonly int Y;
    public readonly int Width;
    public readonly int Height;
    public readonly Dictionary<string, Layer> Layers = new();
    public readonly string Iid;
    public bool Active { get; internal set; } = false;

    public Level(LDtkLevelInstance level)
    {
        Name = level.Identifier;
        X = level.WorldX;
        Y = level.WorldY;
        Width = level.PxWid;
        Height = level.PxHei;
        Iid = level.Iid;
    }
    public Level(string name, int x, int y, int width, int height)
    {
        Name = name;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
    
    public Layer AddLayer(string name)
    {
        if (Layers.ContainsKey(name))
            throw new ArgumentException("A layer with the specified name \"" + name + "\" already exists in level \"" + Name + "\"");
        
        Layers.Add(name, new Layer(name, this));

        return Layers[name];
    }

    public Layer GetLayer(string name)
    {
        try
        {
            return Layers[name];
        }
        catch
        {
            return null;
        }
    }
    
    // TODO: Name is deprecated, refactor into method "Add" below
    public void AddDrawable(string layerName, IDrawable drawable)
    {
        if (!Layers.ContainsKey(layerName))
            throw new ArgumentException("No layer with the specified name \"" + layerName + "\" exists in level \"" + Name + "\"");
        
        Layers[layerName].Add(drawable);
    }

    
    public void AddTile(string layerName, int x, int y, Tile tile)
    {
        if (!Layers.ContainsKey(layerName))
            throw new ArgumentException("No layer with the specified name \"" + layerName + "\" exists in level \"" + Name + "\"");
        
        Layers[layerName].AddTile(x, y, tile);
    }
    

    public void Add(string layerName, IDrawable drawable)
    {
        Layers[layerName].Add(drawable);
    }
    
    public void Add(string layerName, Actor actor)
    {
        Layers[layerName].Add(actor);
    }
    
    public void Add(string layerName, GenericLevelElement genericLevelElement)
    {
        Layers[layerName].Add(genericLevelElement);
    }

    internal void Draw()
    {
        foreach (var layer in Layers.Values)
        {
            layer.Draw();
        }
    }

    internal void Tick()
    {
        foreach (var layer in Layers.Values)
        {
            layer.Tick();
        }
    }

    internal void Activate()
    {
        Active = true;

        foreach (var layer in Layers.Values)
        {
            layer.Activate();
        }
    }

    internal void Deactivate()
    {
        Active = false;
        
        foreach (var layer in Layers.Values)
        {
            layer.Deactivate();
        }
    }
}