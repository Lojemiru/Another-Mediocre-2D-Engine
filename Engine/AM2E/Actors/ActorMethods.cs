using Microsoft.Xna.Framework.Graphics;
using AM2E.Collision;
using System;
using AM2E.Graphics;
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

public abstract partial class Actor : ColliderBase, IDrawable
{
    #region Constructors/deconstructor

    /// <summary>
    /// Standard constructor.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="layer"></param>
    /// <param name="hitbox"></param>
    /// <param name="flipX"></param>
    /// <param name="flipY"></param>
    /// <param name="id"></param>
    protected Actor(int x, int y, Layer layer, Hitbox hitbox = null, bool flipX = false, bool flipY = false, string id = null) : base(x, y, layer)
    {
        ID = id ?? Guid.NewGuid().ToString();
        hitbox ??= GetDefaultHitbox();
        Collider.AddHitbox(hitbox);
        ApplyFlips(flipX, flipY);
    }

    /// <summary>
    /// Constructor from LDtk Entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="layer"></param>
    /// <param name="hitbox"></param>
    protected Actor(LDtkEntityInstance entity, int x, int y, Layer layer, Hitbox hitbox = null) : this(x, y, layer, hitbox, (entity.F & 1) != 0, (entity.F & 2) != 0, entity.Iid)
    {
    }
    
    ~Actor()
    {
        OnCleanup();
    }
    
    #endregion
    
    
    #region Public Methods
    
    /// <summary>
    /// Draws this <see cref="Actor"/> to the supplied <see cref="SpriteBatch"/>.
    /// </summary>
    /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to which this <see cref="Actor"/> should be drawn.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        OnDraw(spriteBatch);
    }
    
    /// <summary>
    /// Returns the appropriate <see cref="SpriteEffects"/> for the current values of <see cref="FlippedX"/> and <see cref="FlippedY"/>.
    /// </summary>
    /// <returns>The corresponding member of <see cref="SpriteEffects"/>, including an "overflow" value for simultaneous horizontal and vertical.</returns>
    public SpriteEffects GetSpriteFlips()
    {
        return (FlippedX ? SpriteEffects.FlipHorizontally : 0) | (FlippedY ? SpriteEffects.FlipVertically : 0);
    }
    
    #endregion
    
    
    #region Internal Methods

    /// <summary>
    /// Calls this <see cref="Actor"/>'s <see cref="OnStep"/> event.
    /// </summary>
    internal void Step()
    {
        OnStep();
    }

    #endregion
    
    
    #region Protected Methods
    
    /// <summary>
    /// Applies the specified axis flips to this <see cref="Actor"/> and its <see cref="Hitbox"/>.
    /// </summary>
    /// <param name="xFlip">Whether this <see cref="Actor"/> is flipped on the X axis.</param>
    /// <param name="yFlip">Whether this <see cref="Actor"/> is flipped on the Y axis.</param>
    protected void ApplyFlips(bool xFlip, bool yFlip)
    {
        FlippedX = xFlip;
        FlippedY = yFlip;
        Collider.ApplyFlips(FlippedX, FlippedY);
    }
    
    /// <summary>
    /// Applies the specified axis flips to this <see cref="Actor"/> and its <see cref="Hitbox"/>.
    /// </summary>
    /// <param name="bits">The flips to be applied in binary format - only the two least significant bits are valid.</param>
    /// <exception cref="ArgumentOutOfRangeException">If the value of <paramref name="bits"/> is greater than decimal 3.</exception>
    protected void ApplyFlipsFromBits(byte bits)
    {
        if (bits > 3)
            throw new ArgumentOutOfRangeException(nameof(bits), "Bits must be equal to or less than decimal 3!");
        
        ApplyFlips((bits & 1) != 0, (bits & 2) != 0);
    }

    #endregion
    
    
    #region Virtual Methods
    
    /// <summary>
    /// Overridable method that gets called when this <see cref="Actor"/> is deconstructed.
    /// </summary>
    protected virtual void OnCleanup() { }

    /// <summary>
    /// Overridable method that gets called every render frame.
    /// </summary>
    /// <param name="spriteBatch"></param>
    protected virtual void OnDraw(SpriteBatch spriteBatch) { }
    
    /// <summary>
    /// Overridable method that gets called when this <see cref="Actor"/>'s <see cref="Level"/> is deactivated.
    /// This will NOT get called for persistent <see cref="Actor"/>s that do not currently have an assigned <see cref="Level"/>!
    /// </summary>
    protected internal virtual void OnLevelDeactivate() { }
    
    /// <summary>
    /// Overridable method that gets called when this <see cref="Actor"/>'s <see cref="Level"/> is activated.
    /// This will NOT get called for persistent <see cref="Actor"/>s that do not currently have an assigned <see cref="Level"/>!
    /// </summary>
    protected internal virtual void OnLevelActivate() { }
    
    /// <summary>
    /// Overridable method that gets called every logical tick.
    /// </summary>
    protected virtual void OnStep() { }
    
    /// <summary>
    /// Overridable method that gets called after this <see cref="Actor"/>'s constructor is run.
    /// </summary>
    protected internal virtual void PostConstructor() { }
    
    #endregion
    
    
    #region Private Methods
    
    /// <summary>
    /// Generates a new 16x16 <see cref="RectangleHitbox"/>. 
    /// </summary>
    /// <returns></returns>
    private static Hitbox GetDefaultHitbox() => new RectangleHitbox(0, 0, 16, 16);

    #endregion
}