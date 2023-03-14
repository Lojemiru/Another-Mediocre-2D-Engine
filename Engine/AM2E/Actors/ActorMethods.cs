﻿using Microsoft.Xna.Framework.Graphics;
using AM2E.Collision;
using System;
using AM2E.Levels;

namespace AM2E.Actors;

// TODO: Surely this could be made a subclass of ActorManager so that step methods etc. could be made protected...
public abstract partial class Actor : IDrawable, ICollider
{
    /// <summary>
    /// Standard constructor.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="hitbox"></param>
    /// <param name="flipX"></param>
    /// <param name="flipY"></param>
    /// <param name="id"></param>
    protected Actor(int x, int y, Hitbox hitbox = null, bool flipX = false, bool flipY = false, string id = null)
    {
        ID = id ?? Guid.NewGuid().ToString();
        // TODO: This is bad code! Will result in a shared hitbox.
        hitbox ??= DefaultHitbox;
        Collider = new Collider(x, y, hitbox);
        X = x;
        Y = y;
        ApplyFlips(flipX, flipY);
        LOIC.Register(this);
    }
        
    /// <summary>
    /// Constructor from LDtk Entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    protected Actor(LDtkEntityInstance entity, int x, int y, Hitbox hitbox = null) : this(x, y, hitbox, (entity.F & 1) != 0, (entity.F & 2) != 0, entity.Iid)
    {
    }

    ~Actor()
    {
        OnCleanup();
    }

    public virtual void PostConstructor()
    {
        // Nothing - we want an empty event so actors don't /have/ to define it.
    }

    public void ApplyFlipsFromBits(int bits)
    {
        ApplyFlips((bits & 1) != 0, (bits & 2) != 0);
    }
    public void ApplyFlips(bool xFlip, bool yFlip)
    {
        FlippedX = xFlip;
        FlippedY = yFlip;
        Collider.ApplyFlips(FlippedX, FlippedY);
    }

    public SpriteEffects GetSpriteFlips()
    {
        if (FlippedX && FlippedY)
            return SpriteEffects.FlipHorizontally & SpriteEffects.FlipVertically;
            
        return FlippedX ? SpriteEffects.FlipHorizontally : (FlippedY ? SpriteEffects.FlipVertically : SpriteEffects.None);
    }
        
    public virtual void OnRoomStart()
    {
        // Nothing - we want an empty event so actors don't /have/ to define it.
    }

    public void Step()
    {
        OnStep();
    }

    protected virtual void OnStep()
    {
        // Nothing - we want an empty event so actors don't /have/ to define it.
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        OnDraw(spriteBatch);
    }

    protected virtual void OnDraw(SpriteBatch spriteBatch)
    {
        // Nothing - we want an empty draw so actors don't /have/ to define it.
    }

    public void Destroy()
    {
        OnDestroy();
        Deregister();
    }

    protected virtual void OnDestroy()
    {
        // Nothing - we want an empty destroy so actors don't /have/ to define it.
    }

    public virtual void OnRoomEnd()
    {
        // Nothing - we want an empty room end so actors don't /have/ to define it.
    }

    protected virtual void OnCleanup()
    {
        // Nothing - we want an empty cleanup event so actors don't /have/ to define it.
    }

    public void Deregister()
    {
        Exists = false;
        Layer.Remove(this);
        // TODO: Does this cause the object to get automatically cleaned up?
    }
}