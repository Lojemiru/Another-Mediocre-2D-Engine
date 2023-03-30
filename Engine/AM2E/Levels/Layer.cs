using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using AM2E.Actors;
using AM2E.Collision;

namespace AM2E.Levels;

public sealed class Layer
{
    // TODO: tiles should be handled under a specific collection or class for easier access in-code.
    // TODO: We can't delete things right now because it will wipe them from the collection before it finishes enumeration, which inevitably crashes.
    public readonly string Name;
    private readonly SpriteBatch spriteBatch = new(EngineCore._graphics.GraphicsDevice);
    public readonly List<IDrawable> Drawables = new();
    public readonly List<Actor> Actors = new();
    public readonly List<ICollider> Colliders = new();
    public readonly List<GenericLevelElement> GenericLevelElements = new();
    private readonly List<GenericLevelElement> genericLevelElementsForRemoval = new();
    private readonly List<GenericLevelElement> genericLevelElementsForAddition = new();
    private bool inTick = false;
    
    public bool Visible = true;
    public readonly Level Level;

    public Layer(string name, Level level)
    {
        Name = name;
        Level = level;
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
        
        actor.Layer?.Remove(actor);
        actor.Layer = this;
        // TODO: Level doesn't get adjusted for other types yet...
        actor.Level = Level;
        Actors.Add(actor);
        Drawables.Add(actor);
        Colliders.Add(actor);
    }

    public void Add(ICollider collider)
    {
        Colliders.Add(collider);
    }

    public void Add(GenericLevelElement genericLevelElement)
    {
        if (inTick)
        {
            QueueForAddition(genericLevelElement);
            return;
        }
        
        GenericLevelElements.Add(genericLevelElement);
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
            case ICollider collider:
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
            case ICollider collider:
                throw new NotImplementedException();
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
        
        Actors.Remove(actor);
        Drawables.Remove(actor);
        Colliders.Remove(actor);
    }

    internal void Remove(GenericLevelElement genericLevelElement)
    {
        if (inTick)
        {
            QueueForRemoval(genericLevelElement);
            return;
        }

        GenericLevelElements.Remove(genericLevelElement);
    }

    internal void Draw()
    {
        if (!Visible) return;
            
        // Sort by texture to avoid constant swaps - this will save performance (particularly on tiles) but nuke depth,
        // but layers are a single depth so we don't care!!!
        spriteBatch.Begin(SpriteSortMode.Texture, samplerState:SamplerState.PointClamp, transformMatrix:Camera.Transform);
        foreach(var drawable in Drawables)
        {
            drawable.Draw(spriteBatch);
        }
        spriteBatch.End();
    }

    internal void Tick()
    {
        inTick = true;
        
        foreach (var actor in Actors)
        {
            actor.Step();
        }
        
        inTick = false;
        
        // TODO: Review addition/removal order.

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
}