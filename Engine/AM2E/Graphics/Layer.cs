﻿using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using AM2E.Actors;
using AM2E.Collision;

namespace AM2E.Graphics;

public class Layer
{
    // TODO: tiles should be handled under a specific collection or class for easier access in-code.
    public readonly string Name;
    private readonly SpriteBatch spriteBatch = new(EngineCore._graphics.GraphicsDevice);
    public readonly List<IDrawable> Drawables = new();
    public readonly List<Actor> Actors = new();
    public readonly List<object> Objects = new();
    public readonly List<ICollider> Colliders = new();
    public bool Visible = true;

    public Layer(string name)
    {
        Name = name;
    }

    public void Add(IDrawable drawable)
    {
        Drawables.Add(drawable);
    }

    public void Add(Actor actor)
    {
        actor.Layer?.Remove(actor);
        actor.Layer = this;
        Actors.Add(actor);
        Drawables.Add(actor);
        Colliders.Add(actor);
    }

    public void Add(object obj)
    {
        Objects.Add(obj);
        if (obj is ICollider collider)
            Colliders.Add(collider);
    }

    public void Remove(IDrawable drawable)
    {
        Drawables.Remove(drawable);
    }

    public void Remove(Actor actor)
    {
        Actors.Remove(actor);
        Drawables.Remove(actor);
        Colliders.Remove(actor);
    }

    public void Draw()
    {
        if (!Visible) return;
            
        // Sort by texture to avoid constant swaps - this will save performance (particularly on tiles) but nuke depth,
        // but layers are a single depth so we don't care!!!
        spriteBatch.Begin(SpriteSortMode.Texture, samplerState:SamplerState.PointClamp, transformMatrix:GameCamera.Transform);
        foreach(var drawable in Drawables)
        {
            drawable.Draw(spriteBatch);
        }
        spriteBatch.End();
    }
}