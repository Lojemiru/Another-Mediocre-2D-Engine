using System;
using AM2E.Actors;
using AM2E.Collision;

namespace AM2E.Levels;

#region Design Notes

/*
 * There are three "layers" of level elements in AM2E. This one resides in-between the GenericLevelElement and Actor.
 *      In addition to everything provided by the GenericLevelElement, the ColliderBase provides a simple means of
 *      interfacing with a Collider, enabling collisions to be detected against children of this class. This does not,
 *      however, provide any kind of "step event" or means of checking for and responding to collisions; without adding
 *      functionality in a child class, this will just sit in one spot and allow more involved classes like the Actor to
 *      detect collisions against it.
 *
 * We override the GenericLevelElement's X and Y properties to pass them down to the Collider's X and Y positions
 *      instead. This is done to ensure that the Collider is never desynced from the ColliderBase's position.
 */

#endregion

public abstract class ColliderBase : GenericLevelElement, ICollider
{
    public Collider Collider { get; }
    
    public new int X
    {
        get => Collider.X;
        set => Collider.X = value;
    }

    public new int Y
    {
        get => Collider.Y;
        set => Collider.Y = value;
    }

    public bool CollisionActive 
        => (Level?.Active ?? true) && Layer is not null;

    public bool FlippedX { get; private set; } = false;
    
    public bool FlippedY { get; private set; } = false;

    protected ColliderBase(int x, int y, Layer layer, Hitbox hitbox = null, bool flipX = false, bool flipY = false, string id = null) 
        : base(x, y, layer, id)
    {
        Collider = new Collider(x, y, this);
        if (hitbox != null)
            Collider.AddHitbox(hitbox);
        ApplyFlips(flipX, flipY);
    }

    protected ColliderBase(LDtkEntityInstance entity, int x, int y, Layer layer, Hitbox hitbox = null)
        : this(x, y, layer, hitbox, (entity.F & 1) != 0, (entity.F & 2) != 0, entity.Iid) { }
    
    /// <summary>
    /// Applies the specified axis flips to this <see cref="ColliderBase"/> and its <see cref="Hitbox"/>.
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
    /// Applies the specified axis flips to this <see cref="ColliderBase"/> and its <see cref="Hitbox"/>.
    /// </summary>
    /// <param name="bits">The flips to be applied in binary format - only the two least significant bits are valid.</param>
    /// <exception cref="ArgumentOutOfRangeException">If the value of <paramref name="bits"/> is greater than decimal 3.</exception>
    protected void ApplyFlipsFromBits(byte bits)
    {
        if (bits > 3)
            throw new ArgumentOutOfRangeException(nameof(bits), "Bits must be equal to or less than decimal 3!");
        
        ApplyFlips((bits & 1) != 0, (bits & 2) != 0);
    }

    internal override void Dispose(bool fromLayer)
    {
        Collider.Dispose();
        base.Dispose(fromLayer);
    }
}