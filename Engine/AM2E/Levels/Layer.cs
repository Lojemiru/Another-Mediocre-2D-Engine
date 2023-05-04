using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using AM2E.Actors;
using AM2E.Graphics;

namespace AM2E.Levels;

public sealed class Layer
{
    public readonly string Name;
    private readonly SpriteBatch spriteBatch = new(EngineCore._graphics.GraphicsDevice);
    public readonly List<IDrawable> Drawables = new();
    public readonly List<Actor> Actors = new();
    public readonly List<ColliderBase> Colliders = new();
    public readonly List<GenericLevelElement> GenericLevelElements = new();
    private readonly List<GenericLevelElement> genericLevelElementsForRemoval = new();
    private readonly List<GenericLevelElement> genericLevelElementsForAddition = new();

    public TileManager TileManager;

    private bool inTick = false;

    public bool Visible = true;
    public readonly Level Level;

    public Layer(string name, Level level)
    {
        Name = name;
        Level = level;
    }
    
    // TODO: Safety for all tile methods

    public void AddTile(int x, int y, Tile tile)
    {
        TileManager ??= new TileManager(Level, tile.Size);
        
        TileManager.AddTile(x, y, tile);
    }

    public Tile GetTile(int x, int y)
    {
        return TileManager.GetTile(x, y);
    }

    public void DeleteTile(int x, int y)
    {
        TileManager.DeleteTile(x, y);
    }

    public void DeleteTiles(int x, int y, int numX, int numY)
    {
        TileManager.DeleteTiles(x, y, numX, numY);
    }

    public void Add(IDrawable drawable)
    {
        Drawables.Add(drawable);
    }

    public void Add(Actor actor)
    {
        if (inTick)
        {
            QueueForAddition(actor);
            return;
        }
        
        SwapLayer(actor);
        
        GenericLevelElements.Add(actor);
        Colliders.Add(actor);
        Actors.Add(actor);
        Drawables.Add(actor);
    }

    public void Add(ColliderBase collider)
    {
        SwapLayer(collider);
        
        GenericLevelElements.Add(collider);
        Colliders.Add(collider);
    }

    public void Add(GenericLevelElement genericLevelElement)
    {
        if (inTick)
        {
            QueueForAddition(genericLevelElement);
            return;
        }
        
        SwapLayer(genericLevelElement);
        
        GenericLevelElements.Add(genericLevelElement);
    }

    private void SwapLayer(GenericLevelElement genericLevelElement)
    {
        genericLevelElement.Layer?.RemoveGeneric(genericLevelElement);
        genericLevelElement.Layer = this;
        genericLevelElement.Level = Level;
    }

    internal void AddGeneric(GenericLevelElement obj)
    {
        if (inTick)
        {
            QueueForAddition(obj);
            return;
        }
        
        switch (obj)
        {
            case Actor actor:
                Add(actor);
                break;
            case ColliderBase collider:
                Add(collider);
                break;
            case { }:
                Add(obj);
                break;
        }
    }

    internal void RemoveGeneric(GenericLevelElement gle)
    {
        if (inTick)
        {
            QueueForRemoval(gle);
            return;
        }
        
        switch (gle)
        {
            case Actor actor:
                Remove(actor);
                break;
            case ColliderBase collider:
                Remove(collider);
                return;
            case {}:
                Remove(gle);
                break;
        }
    }

    private void QueueForAddition(GenericLevelElement gle)
    {
        if (!genericLevelElementsForAddition.Contains(gle))
            genericLevelElementsForAddition.Add(gle);
    }
    
    private void QueueForRemoval(GenericLevelElement gle)
    {
        if (!genericLevelElementsForRemoval.Contains(gle))
            genericLevelElementsForRemoval.Add(gle);
    }

    public void Remove(IDrawable drawable)
    {
        Drawables.Remove(drawable);
    }

    internal void Remove(Actor actor)
    {
        if (inTick) 
        {
            QueueForRemoval(actor);
            return;
        }
        
        DisconnectLayer(actor);

        GenericLevelElements.Remove(actor);
        Colliders.Remove(actor);
        Actors.Remove(actor);
        Drawables.Remove(actor);
    }

    internal void Remove(ColliderBase collider)
    {
        DisconnectLayer(collider);
        
        GenericLevelElements.Remove(collider);
        Colliders.Remove(collider);
    }

    internal void Remove(GenericLevelElement genericLevelElement)
    {
        if (inTick)
        {
            QueueForRemoval(genericLevelElement);
            return;
        }
        
        DisconnectLayer(genericLevelElement);

        GenericLevelElements.Remove(genericLevelElement);
    }

    private void DisconnectLayer(GenericLevelElement genericLevelElement)
    {
        if (genericLevelElement.Layer != this)
            return;
        
        genericLevelElement.Layer = null;
        genericLevelElement.Level = null;
    }

    internal void Draw()
    {
        if (!Visible) return;
        
        spriteBatch.Begin(SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp, transformMatrix:Camera.Transform);
        foreach(var drawable in Drawables)
        {
            drawable.Draw(spriteBatch);
        }
        
        Renderer.DebugRender(spriteBatch);

        TileManager?.Draw(spriteBatch);
        
        spriteBatch.End();
    }

    internal void PreTick()
    {
        inTick = true;
        
        foreach (var actor in Actors)
        {
            actor.PreStep();
        }
    }

    internal void Tick()
    {
        foreach (var actor in Actors)
        {
            actor.Step();
        }
    }

    internal void PostTick()
    {
        foreach (var actor in Actors)
        {
            actor.PostStep();
        }
        
        inTick = false;

        foreach (var gle in genericLevelElementsForRemoval)
        {
            RemoveGeneric(gle);
        }
        
        genericLevelElementsForRemoval.Clear();

        foreach (var gle in genericLevelElementsForAddition)
        {
            AddGeneric(gle);
        }
        
        genericLevelElementsForAddition.Clear();

        TileManager?.Step();
    }

    internal void Activate()
    {
        foreach (var actor in Actors)
        {
            actor.OnLevelActivate();
        }
    }

    internal void Deactivate()
    {
        foreach (var actor in Actors)
        {
            actor.OnLevelDeactivate();
        }
    }

    internal void Dispose()
    {
        foreach (var genericLevelElement in GenericLevelElements)
        {
            genericLevelElement.Dispose(true);
        }
    }
}