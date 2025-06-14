using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using AM2E.Actors;
using AM2E.Graphics;
using AM2E.Networking;

namespace AM2E.Levels;

public sealed class Layer
{
    public readonly string Name;
    public readonly List<IDrawable> Drawables = new();
    public readonly List<Actor> Actors = new();
    public readonly List<ColliderBase> Colliders = new();
    public readonly List<GenericLevelElement> GenericLevelElements = new();
    private readonly List<GenericLevelElement> genericLevelElementsForRemoval = new();
    private readonly List<GenericLevelElement> genericLevelElementsForAddition = new();

    public TileManager? TileManager { get; private set; }

    internal bool InTick = false;

    public bool RenderTiles = true;
    public bool Visible = true;
    public readonly Level Level;

    public event Action<SpriteBatch, Layer> OnPreRender = (_, _) => { };

    public event Action<SpriteBatch, Layer> OnPostRender = (_, _) => { };

    public Layer(string name, Level level)
    {
        Name = name;
        Level = level;
    }

    public void AddTile(int x, int y, Tile tile)
    {
        TileManager ??= new TileManager(Level, tile.TilesetSprite, tile.Size);
        
        TileManager.AddTile(x, y, tile);
    }

    public Tile? GetTile(int x, int y)
    {
        return TileManager?.GetTile(x, y);
    }

    public Tile[,]? GetTiles()
    {
        return TileManager?.Tiles;
    }

    public Sprite? GetTilesetSprite()
    {
        return TileManager?.TilesetSprite;
    }

    public void DeleteTile(int x, int y)
    {
        TileManager?.DeleteTile(x, y);
    }

    public void DeleteTiles(int x, int y, int numX, int numY)
    {
        TileManager?.DeleteTiles(x, y, numX, numY);
    }

    public void EradicateTiles()
    {
        TileManager = null;
    }

    public void DoTileRender(SpriteBatch spriteBatch, int offsetX = 0, int offsetY = 0, int distancePastCamera = 0)
    {
        if (RenderTiles)
            TileManager?.Draw(spriteBatch, offsetX, offsetY, distancePastCamera);
    }

    public void Add(IDrawable drawable)
    {
        Drawables.Add(drawable);
    }

    public void Add(Actor actor)
    {
        if (InTick)
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
        if (InTick)
        {
            QueueForAddition(collider);
            return;
        }
        
        SwapLayer(collider);

        GenericLevelElements.Add(collider);
        Colliders.Add(collider);
    }

    public void Add(GenericLevelElement genericLevelElement)
    {
        if (InTick)
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
        if (InTick)
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

    public void RemoveGeneric(GenericLevelElement gle)
    {
        if (InTick)
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

    public void Remove(Actor actor)
    {
        if (InTick) 
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

    public void Remove(ColliderBase collider)
    {
        if (InTick)
        {
            QueueForRemoval(collider);
            return;
        }

        DisconnectLayer(collider);
        
        GenericLevelElements.Remove(collider);
        Colliders.Remove(collider);
    }

    public void Remove(GenericLevelElement genericLevelElement)
    {
        if (InTick)
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

    internal void Draw(SpriteBatch spriteBatch)
    {
        if (!Visible) return;

        OnPreRender(spriteBatch, this);

        foreach(var drawable in Drawables)
        {
            // Apply culling.
            if (drawable.CullingBounds is not null)
            {
                var val = drawable.CullingBounds.Value;
                if (drawable.X - val.L > Camera.BoundRight ||
                    drawable.X + val.R < Camera.BoundLeft ||
                    drawable.Y - val.U > Camera.BoundBottom ||
                    drawable.Y + val.D < Camera.BoundTop)
                    continue;
            }

            drawable.Draw(spriteBatch);
        }

        DoTileRender(spriteBatch);

        OnPostRender(spriteBatch, this);
    }

    internal void PreTick(bool isFastForward)
    {
        InTick = true;
        
        foreach (var actor in Actors)
        {
            if (isFastForward && actor is not INetSynced)
                continue;

            actor.PreStep();
        }
    }

    internal void Tick(bool isFastForward)
    {
        foreach (var actor in Actors)
        {
            if (isFastForward && actor is not INetSynced)
                continue;

            actor.Step();
        }
        
        TileManager?.Step();
    }

    internal void PostTick(bool isFastForward)
    {
        foreach (var actor in Actors)
        {
            if (isFastForward && actor is not INetSynced)
                continue;

            actor.PostStep();
        }
        
        InTick = false;

        HandleAdditionAndRemoval();
    }

    internal void HandleAdditionAndRemoval()
    {
        foreach (var gle in genericLevelElementsForAddition)
        {
            AddGeneric(gle);
        }
        
        genericLevelElementsForAddition.Clear();

        foreach (var gle in genericLevelElementsForRemoval)
        {
            RemoveGeneric(gle);
        }
        
        genericLevelElementsForRemoval.Clear();
    }

    internal void Activate()
    {
        InTick = true;
        foreach (var actor in Actors)
        {
            actor.OnLevelActivate();
        }
        InTick = false;
    }

    internal void Deactivate()
    {
        InTick = true;
        foreach (var actor in Actors)
        {
            actor.OnLevelDeactivate();
        }
        InTick = false;
    }

    internal void Dispose()
    {
        foreach (var genericLevelElement in GenericLevelElements)
        {
            genericLevelElement.Dispose(true);
        }
    }
}