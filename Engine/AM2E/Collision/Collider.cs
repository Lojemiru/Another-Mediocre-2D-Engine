using AM2E.Levels;
using RTree;
// ReSharper disable MemberCanBePrivate.Global

namespace AM2E.Collision;

public sealed class Collider
{
    public Rectangle? Bounds;
    
    private enum Axis
    {
        X,
        Y
    }

    private int xInternal;
    private int yInternal;
    public int X
    {
        get => xInternal;
        set
        {
            xInternal = value;
            SyncHitboxPositions();
        }
    }
    public int Y
    {
        get => yInternal;
        set
        {
            yInternal = value;
            SyncHitboxPositions();
        }
    }
    
    public int VelX => vel[0];
    public int VelY => vel[1];
    
    private readonly Dictionary<Type, object> events = new();

    private int checkX = 0;
    private int checkY = 0;

    private static readonly CollisionDirection[] DirsH = [CollisionDirection.Left, CollisionDirection.None, CollisionDirection.Right
    ];
    private static readonly CollisionDirection[] DirsV = [CollisionDirection.Up, CollisionDirection.None, CollisionDirection.Down
    ];

    private readonly bool[] continueMovement = [true, true];
    public double SubVelX
    {
        get => subVel[0];
        set => subVel[0] = value;
    }
    public double SubVelY
    {
        get => subVel[1];
        set => subVel[1] = value;
    }

    private readonly double[] subVel = [0d, 0d];
    private readonly int[] vel = [0, 0];

    public bool InMovement { get; private set; } = false;

    private bool disposed = false;
    
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public Action? OnSubstep { get; set; }
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public Action? AfterSubstep { get; set; }

    public CollisionDirection Direction { get; private set; } = CollisionDirection.None;
    
    /// <summary>
    /// Records the hitbox that was last matched in a normal collision substep. Useful for filtering behavior based on
    /// which hitbox was matched.
    /// </summary>
    public Hitbox? CollidingHitbox = null;
    
    private readonly List<Hitbox> hitboxes = [];
    
    private readonly ColliderBase parent;
    private bool first = true;
    private bool syncing = false;
        
    public Hitbox? GetHitbox(int id)
    {
        return (0 <= id && id < hitboxes.Count) ? hitboxes[id] : null;
    }

    public int AddHitbox(Hitbox hitbox)
    {
        hitboxes.Add(hitbox);
        hitbox.Collider = this;
        hitbox.X = X;
        hitbox.Y = Y;
        hitbox.ApplyFlips(FlippedX, FlippedY);
        
        SyncHitboxPositions();
        
        return hitboxes.Count - 1;
    }

    public void RemoveHitbox(Hitbox hitbox)
    {
        hitboxes.Remove(hitbox);
        hitbox.Collider = null;
        SyncHitboxPositions();
    }
        
    public bool FlippedX { get; private set; } = false;
    public bool FlippedY { get; private set; } = false;

    public void ApplyFlipsFromBits(int bits)
    {
        ApplyFlips((bits & 1) != 0, (bits & 2) != 0);
    }
    
    public void ApplyFlips(bool xFlip, bool yFlip)
    {
        if (disposed)
            return;
        
        // This has not broken yet, but I had a note here for concerns relating to addition/removal from the RTree.
        // If something is broken that's probably it :)

        if (!first)
            LOIC.RTree.Delete(Bounds, parent);
        
        FlippedX = xFlip;
        FlippedY = yFlip;
        
        if (hitboxes.Count <= 0) 
            return;

        syncing = true;
        
        var l = int.MaxValue;
        var r = int.MinValue;
        var u = int.MaxValue;
        var d = int.MinValue;

        foreach (var hitbox in hitboxes)
        {
            hitbox.ApplyFlips(FlippedX, FlippedY);
            l = Math.Min(l, hitbox.BoundLeft);
            r = Math.Max(r, hitbox.BoundRight);
            u = Math.Min(u, hitbox.BoundTop);
            d = Math.Max(d, hitbox.BoundBottom);
        }
        
        Bounds = new Rectangle(l, u, r, d);
            
        LOIC.RTree.Add(Bounds, parent);
        first = false;
        syncing = false;
    }

    public void ApplyRotation(float angle)
    {
        if (disposed)
            return;
        
        if (!first)
            LOIC.RTree.Delete(Bounds, parent);
        
        if (hitboxes.Count <= 0) 
            return;

        syncing = true;
        
        var l = int.MaxValue;
        var r = int.MinValue;
        var u = int.MaxValue;
        var d = int.MinValue;
        
        foreach (var hitbox in hitboxes)
        {
            if (hitbox is PolygonHitbox p)
                p.ApplyRotation(angle);
            
            l = Math.Min(l, hitbox.BoundLeft);
            r = Math.Max(r, hitbox.BoundRight);
            u = Math.Min(u, hitbox.BoundTop);
            d = Math.Max(d, hitbox.BoundBottom);
        }
        
        Bounds = new Rectangle(l, u, r, d);
            
        LOIC.RTree.Add(Bounds, parent);
        first = false;
        syncing = false;
    }
    
    internal void SyncHitboxPositions()
    {
        if (disposed || syncing)
            return;
        
        if (!first)
            LOIC.RTree.Delete(Bounds, parent);

        if (hitboxes.Count <= 0) 
            return;

        syncing = true;
        
        var l = int.MaxValue;
        var r = int.MinValue;
        var u = int.MaxValue;
        var d = int.MinValue;
            
        foreach (var hitbox in hitboxes)
        {
            hitbox.X = X;
            hitbox.Y = Y;
            l = Math.Min(l, hitbox.BoundLeft);
            r = Math.Max(r, hitbox.BoundRight);
            u = Math.Min(u, hitbox.BoundTop);
            d = Math.Max(d, hitbox.BoundBottom);
        }

        Bounds = new Rectangle(l, u, r, d);
            
        LOIC.RTree.Add(Bounds, parent);
        first = false;
        syncing = false;
    }

    internal void Dispose()
    {
        if (disposed)
            return;

        if (hitboxes.Count > 0)
        {
            LOIC.RTree.Delete(Bounds, parent);
        }

        disposed = true;
    }

    internal Collider(int x, int y, ColliderBase parent)
    {
        this.parent = parent;
        X = x;
        Y = y;
    }
    
    internal Collider(int x, int y, ColliderBase parent, Hitbox hitbox)
    {
        this.parent = parent;
        X = x;
        Y = y;
        AddHitbox(hitbox);
    }

    public void Add<T>(Action<T> callback) where T : ICollider
    {
        events.Add(typeof(T), callback);
    }

    private static readonly int[] Sign = [0, 0];
    
    public void MoveAndCollide(double xVel, double yVel)
    {
        const int x = (int)Axis.X;
        const int y = (int)Axis.Y;
            
        // Add velocity to subVel
        subVel[x] += xVel;
        subVel[y] += yVel;

        // Cast off decimals
        vel[x] = (int)subVel[x];
        vel[y] = (int)subVel[y];

        // Reduce subVel by the integer amount we removed
        subVel[x] -= vel[x];
        subVel[y] -= vel[y];
            
        InMovement = true;
        
        // If we're not moving, do a static check and return.
        if (vel[x] == 0 && vel[y] == 0)
        {
            CheckAndRunAll(0, 0);
            return;
        }

        // Determine dominant and subordinate axis
        int domAxis = x,
            subAxis = y;

        try
        {
            if (Math.Abs(vel[y]) > Math.Abs(vel[x]))
            {
                domAxis = y;
                subAxis = x;
            }
        }
        catch (OverflowException e)
        {
            throw new OverflowException("Collider.MoveAndCollider(xVel, yVel): one of the arguments became equal to the smallest possible integer. Please check your inputs.", e);
        }

        var subIncrement = false;
        var domCurrent = 0;
        var subCurrent = 0;
        
        Sign[0] = Math.Sign(vel[x]);
        Sign[1] = Math.Sign(vel[y]);
        
        var domAbs = Math.Abs(vel[domAxis]);

        for (var i = 0; i < domAbs; ++i)
        {
            if (continueMovement[domAxis] && (domCurrent != vel[domAxis]))
            {
                Direction = (domAxis == x) ? DirsH[Sign[x] + 1] : DirsV[Sign[y] + 1];

                CheckAndRunAll((domAxis == x) ? Sign[x] : 0, (domAxis == y) ? Sign[y] : 0);

                // Process position
                var domMult = continueMovement[domAxis] ? Sign[domAxis] : 0;
                domCurrent += domMult;

                if (domAxis == x)
                    X += domMult;
                else
                    Y += domMult;

                var subCurrentLast = subCurrent;
                
                subCurrent = vel[subAxis] * domCurrent / vel[domAxis];
                subIncrement = (subCurrent != subCurrentLast);
                // Prevents stupid slidey shenanigans. At least that's what I said in the LHC...
                subCurrent = subCurrentLast;
            }

            if ((subIncrement || !continueMovement[domAxis]) && continueMovement[subAxis] && (subCurrent != vel[subAxis]))
            {
                Direction = (subAxis == x) ? DirsH[Sign[x] + 1] : DirsV[Sign[y] + 1];

                CheckAndRunAll((subAxis == x) ? Sign[x] : 0, (subAxis == y) ? Sign[y] : 0);

                // Process position
                var subMult = continueMovement[subAxis] ? Sign[subAxis] : 0;
                subCurrent += subMult;

                if (subAxis == x)
                    X += subMult;
                else
                    Y += subMult;
            }

            AfterSubstep?.Invoke();

            // Early exit if stopped on both axes
            if (!continueMovement[x] && !continueMovement[y])
                break;
        }

        // Reset movement variables
        Direction = CollisionDirection.None;
        continueMovement[x] = true;
        continueMovement[y] = true;

        InMovement = false;
    }

    // TODO: Should we provide the ability to stop all further processing of a specific collision event?
    // These don't stop processing of multiple of the same interface that were hit at once...
    
    public void StopX()
    {
        if (!InMovement)
            throw new InvalidOperationException("Stop calls may only be called from a collision callback!");

        continueMovement[0] = false;
    }

    public void StopY()
    {
        if (!InMovement)
            throw new InvalidOperationException("Stop calls may only be called from a collision callback!");

        continueMovement[1] = false;
    }

    public void Stop()
    {
        StopX();
        StopY();
    }

    public bool DirectionHorizontal()
    {
        return Direction is CollisionDirection.Left or CollisionDirection.Right;
    }

    public bool DirectionVertical()
    {
        return Direction is CollisionDirection.Up or CollisionDirection.Down;
    }

    private void CheckAndRunAll(int x, int y)
    {
        checkX = x + X;
        checkY = y + Y;
        
        OnSubstep?.Invoke();
    }

    public void CheckAndRun<T>() where T : ICollider
    {
        var colliders = (List<T>)IntersectsAllAt<T>(checkX, checkY);
        
        if (colliders.Count == 0)
            return;
        
        if (!events.ContainsKey(typeof(T)))
            throw new KeyNotFoundException("No collision event registered for '" + typeof(T) + "'. Ensure that you are using Collider.Add() to register a collision event before running Collider.CheckAndRun<T>().");
        
        var ev = (Action<T>)events[typeof(T)];

        foreach (var col in colliders)
        {
            ev(col);
        }
    }
        
    public T? IntersectsAt<T>(int x, int y) where T : class, ICollider
    {
        var prevX = X;
        var prevY = Y;
        X = x;
        Y = y;

        var output = LOIC.CheckCollider<T>(this);

        X = prevX;
        Y = prevY;

        return output;
    }

    public IEnumerable<T> IntersectsAllAt<T>(int x, int y) where T : ICollider
    {
        var prevX = X;
        var prevY = Y;
        X = x;
        Y = y;

        var output = LOIC.CheckAllColliders<T>(this);

        X = prevX;
        Y = prevY;

        return output;
    }

    public bool IntersectingAt<T>(int x, int y) where T : class, ICollider
    {
        return IntersectsAt<T>(x, y) != null;
    }

    public bool Intersects<T>(Collider col) where T : ICollider
    {
        if (col == this)
            return false;
        
        // Return whether one of our hitboxes that is targeting the given interface
        // can find a hitbox bound to that interface AND is intersecting.
        foreach (var myHitbox in hitboxes)
        {
            if (!myHitbox.IsTargetingInterface<T>()) continue;
            foreach (var hitbox in col.hitboxes)
            {
                if (hitbox.IsBoundToInterface<T>() && myHitbox.Intersects(hitbox))
                {
                    col.CollidingHitbox = hitbox;
                    return true;
                }
            }
        }

        return false;
    }

    public bool ContainsPoint<T>(int x, int y) where T : ICollider
    {
        // Return whether one of our Hitboxes that is bound to the interface AND contains the point.
        foreach (var myHitbox in hitboxes)
        {
            if (myHitbox.IsBoundToInterface<T>() && myHitbox.ContainsPoint(x, y))
                return true;
        }

        return false;
    }

    private static RectangleHitbox testRectangle = new(1, 1);
    
    public bool IntersectsRectangle<T>(int x1, int y1, int x2, int y2) where T : ICollider
    {
        testRectangle.X = x1;
        testRectangle.Y = y1;
        testRectangle.Resize(1 + x2 - x1, 1 + y2 - y1);

        foreach (var hitbox in hitboxes)
        {
            if (hitbox.IsBoundToInterface<T>() && hitbox.Intersects(testRectangle))
                return true;
        }
        
        return false;
    }

    public bool IsIntersectedBy<T>(Hitbox hitbox) where T : ICollider
    {
        // First, check if incoming Hitbox is actually targeting this interface.
        // Then, return whether one of our Hitboxes that is bound to the interface AND is intersecting incoming Hitbox.
        if (!hitbox.IsTargetingInterface<T>())
            return false;
            
        foreach (var myHitbox in hitboxes)
        {
            if (myHitbox.IsBoundToInterface<T>() && myHitbox.Intersects(hitbox))
                return true;
        }

        return false;
    }

    public bool IsIntersectedByLine<T>(int x1, int y1, int x2, int y2) where T : ICollider
    {
        foreach (var hitbox in hitboxes)
        {
            if (hitbox.IsBoundToInterface<T>() && hitbox.IntersectsLine(x1, y1, x2, y2))
                return true;
        }
        
        return false;
    }
}