using Microsoft.Xna.Framework.Graphics;
using AM2E.Collision;
using System;
using AM2E.Levels;

namespace AM2E.Actors;

#region Design Notes

/*
 * Regarding the decision not to use C# events for post-constructor, step event, draw, etc.:
 *      Simply doesn't make sense for the flexibility I desire. The power to completely override when base is called
 *      in each child of a user-defined Actor is an incredibly useful OOP workflow and I don't want to force bad
 *      design patterns to get back to that for the sake of using events. If some fool calls their own OnStep method
 *      manually in their child class, that's on them.
 *
 * Currently, the internal/internal protected modifiers don't do much.
 *      When the engine is eventually split into a library instead of sharing an assembly with GameContent, this will
 *      prevent bad design patterns where end users call the top-level step events etc. from their own code.
 */

#endregion

public abstract partial class Actor : IDrawable, ICollider
{
    #region Constructors/deconstructor
    
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
        hitbox ??= GetDefaultHitbox();
        Collider = new Collider(x, y, hitbox);
        X = x;
        Y = y;
        ApplyFlips(flipX, flipY);
    }

    /// <summary>
    /// Constructor from LDtk Entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="hitbox"></param>
    protected Actor(LDtkEntityInstance entity, int x, int y, Hitbox hitbox = null) : this(x, y, hitbox, (entity.F & 1) != 0, (entity.F & 2) != 0, entity.Iid)
    {
    }
    
    ~Actor()
    {
        OnCleanup();
    }
    
    #endregion
    
    
    #region Public Methods
    
    public void Draw(SpriteBatch spriteBatch)
    {
        OnDraw(spriteBatch);
    }

    public SpriteEffects GetSpriteFlips()
    {
        if (FlippedX && FlippedY)
            return SpriteEffects.FlipHorizontally & SpriteEffects.FlipVertically;
            
        return FlippedX ? SpriteEffects.FlipHorizontally : (FlippedY ? SpriteEffects.FlipVertically : SpriteEffects.None);
    }
    
    #endregion
    
    
    #region Internal Methods
    
    internal void Deregister()
    {
        Exists = false;
        Layer.Remove(this);
        // TODO: Does this cause the object to get automatically cleaned up? We probably want to do it manually anyway.
        // TODO: Need to factor persistent behavior into this.
    }
    
    internal void Step()
    {
        OnStep();
    }

    #endregion
    
    
    #region Protected Methods
    
    protected void ApplyFlips(bool xFlip, bool yFlip)
    {
        FlippedX = xFlip;
        FlippedY = yFlip;
        Collider.ApplyFlips(FlippedX, FlippedY);
    }
    
    protected void ApplyFlipsFromBits(byte bits)
    {
        ApplyFlips((bits & 1) != 0, (bits & 2) != 0);
    }
    
    protected internal void Destroy()
    {
        OnDestroy();
        Deregister();
    }

    #endregion
    
    
    #region Virtual Methods
    
    protected virtual void OnCleanup()
    {
        // Nothing - we want an empty cleanup event so actors don't /have/ to define it.
    }
    
    protected virtual void OnDestroy()
    {
        // Nothing - we want an empty destroy so actors don't /have/ to define it.
    }
    
    protected virtual void OnDraw(SpriteBatch spriteBatch)
    {
        // Nothing - we want an empty draw so actors don't /have/ to define it.
    }
    
    protected internal virtual void OnRoomEnd()
    {
        // Nothing - we want an empty room end so actors don't /have/ to define it.
    }
    
    protected internal virtual void OnRoomStart()
    {
        // Nothing - we want an empty event so actors don't /have/ to define it.
    }
    
    protected virtual void OnStep()
    {
        // Nothing - we want an empty event so actors don't /have/ to define it.
    }
    
    protected internal virtual void PostConstructor()
    {
        // Nothing - we want an empty event so actors don't /have/ to define it.
    }
    
    #endregion
    
    
    #region Private Methods
    
    private static Hitbox GetDefaultHitbox() => new RectangleHitbox(0, 0, 16, 16);

    #endregion
}