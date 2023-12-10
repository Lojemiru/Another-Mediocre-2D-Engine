using System;
using Microsoft.Xna.Framework.Graphics;
using AM2E.Collision;
using AM2E.Levels;

namespace AM2E.Actors;

#region Design Notes

/*
 * There are three "layers" of level elements in AM2E. This is the most derived, and thus the most complex. In addition
 *      to everything provided by ColliderBase and GenericLevelElement, the Actor has functionality for drawing, various
 *      active events (step, level load/unload, etc.), and rendering. While it does not appear that much more complex on
 *      its own, the overhead to trigger these events makes them much more costly at runtime than a ColliderBase; if you
 *      don't need these events, consider using that class instead.
 * 
 * Regarding the decision not to use C# events for step event, draw, etc.:
 *      Simply doesn't make sense for the flexibility I desire. The power to completely override when base is called
 *      in each child of a user-defined Actor is an incredibly useful OOP workflow and I don't want to force bad
 *      design patterns to get back to that for the sake of using events. If some fool calls their own OnStep method
 *      manually in their child class, that's on them.
 */

#endregion

public abstract class Actor : ColliderBase, IDrawable
{
    public bool Persistent { get; private set; } = false;
    
    /// <summary>
    /// The angle of this <see cref="Actor"/>, in degrees clockwise from East.
    /// </summary>
    public float Angle = 0;

    public static Func<bool> DefaultPauseCondition = () => false;

    public readonly bool UsePauseCondition = true;

    public Func<bool> PauseCondition = null;

    private float alpha = 1;
    public float Alpha
    {
        get => alpha;
        set => alpha = Math.Clamp(value, 0, 1);
    }
    
    #region Constructors

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
    protected Actor(int x, int y, Layer layer, Hitbox hitbox = null, bool flipX = false, bool flipY = false,
        string id = null)
        : base(x, y, layer, hitbox, flipX, flipY, id) { }

    /// <summary>
    /// Constructor from LDtk Entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="layer"></param>
    /// <param name="hitbox"></param>
    protected Actor(LDtkEntityInstance entity, int x, int y, Layer layer, Hitbox hitbox = null)
        : this(x, y, layer, hitbox, (entity.F & 1) != 0, (entity.F & 2) != 0, entity.Iid) { }
    
    
    #endregion

    #region Private Methods
    
    private bool IsPaused()
    {
        // If we don't pause at all (control objects, audio), return immediately.
        if (!UsePauseCondition)
            return false;

        // Otherwise, try our custom pause condition; if that fails, return default;
        return PauseCondition?.Invoke() ?? DefaultPauseCondition();
    }
    
    #endregion
    
    #region Public Methods

    /// <summary>
    /// Destroys this <see cref="Actor"/>, running its <see cref="OnDestroy"/> and <see cref="GenericLevelElement.Dispose"/> methods.
    /// </summary>
    public void Destroy()
    {
        OnDestroy();
        Dispose();
    }
    
    /// <summary>
    /// Draws this <see cref="Actor"/> to the supplied <see cref="SpriteBatch"/>.
    /// </summary>
    /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to which this <see cref="Actor"/> should be drawn.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        OnDraw(spriteBatch);
    }
    
    public static Actor GetActor(string id)
    {
        var element = GenericLevelElement.GetElement(id);
        return element is Actor actor ? actor : null;
    }
    
    /// <summary>
    /// Returns the appropriate <see cref="SpriteEffects"/> for the current values of <see cref="FlippedX"/> and <see cref="FlippedY"/>.
    /// </summary>
    /// <returns>The corresponding member of <see cref="SpriteEffects"/>, including an "overflow" value for simultaneous horizontal and vertical.</returns>
    public SpriteEffects GetSpriteFlips()
    {
        return (FlippedX ? SpriteEffects.FlipHorizontally : 0) | (FlippedY ? SpriteEffects.FlipVertically : 0);
    }

    public void SetPersistent(bool persistent)
    {
        if (Persistent == persistent)
            return;
        
        Persistent = persistent;
        
        if (Persistent)
            ActorManager.PersistentActors.Add(ID, this);
        else
            ActorManager.PersistentActors.Remove(ID);
    }
    
    #endregion
    
    
    #region Internal Methods

    /// <summary>
    /// Calls this <see cref="Actor"/>'s <see cref="OnStep"/> event.
    /// </summary>
    internal void Step()
    {
        if (!IsPaused())
            OnStep();
    }

    /// <summary>
    /// Calls this <see cref="Actor"/>'s <see cref="OnPreStep"/> event.
    /// </summary>
    internal void PreStep()
    {
        if (!IsPaused())
            OnPreStep();
    }

    internal void PostStep()
    {
        if (!IsPaused())
            OnPostStep();
    }

    #endregion


    #region Virtual Methods

    /// <summary>
    /// Overridable method that gets called when this <see cref="Actor"/> is <see cref="Destroy"/>ed.
    /// </summary>
    protected virtual void OnDestroy() { }

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
    protected virtual void OnPreStep() { }
    
    /// <summary>
    /// Overridable method that gets called every logical tick.
    /// </summary>
    protected virtual void OnStep() { }
    
    protected virtual void OnPostStep() { }

    #endregion
}