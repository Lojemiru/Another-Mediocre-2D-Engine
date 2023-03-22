using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using AM2E.Actors;
using AM2E.Collision;
using AM2E.Levels;

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

    public void Add(object obj)
    {
        switch (obj)
        {
            case Actor actor:
                Add(actor);
                break;
            case ICollider collider:
                Add(collider);
                break;
            case IDrawable drawable:
                Add(drawable);
                break;
            default:
                Objects.Add(obj);
                break;
        }
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

    public void Remove(ICollider collider)
    {
        Colliders.Remove(collider);
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