﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace AM2E.Collision;

public enum CollisionDirection
{
    None,
    Left,
    Right,
    Up,
    Down
}
public class Collider
{
    private enum Axis
    {
        X,
        Y
    }

    private int x;
    private int y;
    public int X
    {
        get => x;
        set
        {
            x = value;
            SyncHitboxPositions();
        }
    }
    public int Y
    {
        get => y;
        set
        {
            y = value;
            SyncHitboxPositions();
        }
    }
    
    public int VelX => vel[0];
    public int VelY => vel[1];

    private readonly ArrayList events = new();

    private int checkX = 0;
    private int checkY = 0;

    private static readonly CollisionDirection[] DirsH = { CollisionDirection.Left, CollisionDirection.None, CollisionDirection.Right };
    private static readonly CollisionDirection[] DirsV = { CollisionDirection.Up, CollisionDirection.None, CollisionDirection.Down };

    private bool[] continueMovement = { false, false };
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

    private readonly double[] subVel = { 0d, 0d };
    private readonly int[] vel = { 0, 0 };

    private bool inMovement = false;

    public Action OnSubstep { get; set; }
    public Action AfterSubstep { get; set; } = () => { };

    public CollisionDirection Direction { get; private set; } = CollisionDirection.None;
    private readonly List<Hitbox> hitboxes = new();
        
    public Hitbox GetHitbox(int id)
    {
        return (0 <= id && id < hitboxes.Count) ? hitboxes[id] : null;
    }

    public int AddHitbox(Hitbox hitbox)
    {
        hitboxes.Add(hitbox);
        hitbox.X = X;
        hitbox.Y = Y;
        return hitboxes.Count - 1;
    }

    public void RemoveHitbox(Hitbox hitbox)
    {
        hitboxes.Remove(hitbox);
    }
        
    public bool FlippedX { get; protected set; } = false;
    public bool FlippedY { get; protected set; } = false;

    public void ApplyFlipsFromBits(int bits)
    {
        ApplyFlips((bits & 1) != 0, (bits & 2) != 0);
    }
    public void ApplyFlips(bool xFlip, bool yFlip)
    {
        FlippedX = xFlip;
        FlippedY = yFlip;
        foreach (var hitbox in hitboxes)
        {
            hitbox.ApplyFlips(FlippedX, FlippedY);
        }
    }

    private void SyncHitboxPositions()
    {
        foreach (var hitbox in hitboxes)
        {
            hitbox.X = X;
            hitbox.Y = Y;
        }
    }

    public Collider(int x, int y)
    {
        X = x;
        Y = y;
    }
    
    public Collider(int x, int y, Hitbox hitbox)
    {
        X = x;
        Y = y;
        AddHitbox(hitbox);
    }

    public void Add<T>(Action<T> callback) where T : ICollider
    {
        events.Add(callback);
    }

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
            
        // If we're not moving, do a static check and return.
        if (vel[x] == 0 && vel[y] == 0)
        {
            CheckAndRunAll(0, 0);
            return;
        }
            
        inMovement = true;

        // Determine dominant and subordinate axis
        int domAxis = x,
            subAxis = y;

        if (Math.Abs(vel[y]) > Math.Abs(vel[x]))
        {
            domAxis = y;
            subAxis = x;
        }

        var subIncrement = false;
        var domCurrent = 0;
        var subCurrent = 0;

        var domAbs = Math.Abs(vel[domAxis]);
        var subAbs = Math.Abs(vel[subAxis]);

        var sign = new int[] { Math.Sign(vel[x]), Math.Sign(vel[y]) };

        for (var i = 0; i < domAbs; ++i)
        {
            if (continueMovement[domAxis] && (domCurrent != vel[domAxis]))
            {
                Direction = (domAxis == x) ? DirsH[sign[x] + 1] : DirsV[sign[y] + 1];

                CheckAndRunAll((domAxis == x) ? sign[x] : 0, (domAxis == y) ? sign[y] : 0);

                // Process position
                var domMult = continueMovement[domAxis] ? sign[domAxis] : 0;
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
                Direction = (subAxis == x) ? DirsH[sign[x] + 1] : DirsV[sign[y] + 1];

                CheckAndRunAll((subAxis == x) ? sign[x] : 0, (subAxis == y) ? sign[y] : 0);

                // Process position
                var subMult = continueMovement[subAxis] ? sign[subAxis] : 0;
                subCurrent += subMult;

                if (subAxis == x)
                    X += subMult;
                else
                    Y += subMult;
            }

            AfterSubstep();

            // Early exit if stopped on both axes
            if (!continueMovement[x] && !continueMovement[y])
                break;
        }

        // Reset movement variables
        Direction = CollisionDirection.None;
        continueMovement[x] = true;
        continueMovement[y] = true;

        inMovement = false;
    }

    public void StopX()
    {
        if (!inMovement)
            throw new InvalidOperationException("Stop calls may only be called from a collision callback!");

        continueMovement[0] = false;
    }

    public void StopY()
    {
        if (!inMovement)
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

        OnSubstep();
    }

    public void CheckAndRun<T>() where T : ICollider
    {
        var col = Check<T>(checkX, checkY);

        if (col == null) return;
        foreach (var ob in events)
        {
            if (ob is not Action<T> ev) continue;
            ev((T)col);
            return;
        }
    }
        
    public ICollider Check<T>(int x, int y) where T : ICollider
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

    public bool Intersects<T>(Collider col) where T : ICollider
    {
        // Return whether one of our hitboxes that is targeting the given interface
        // can find a hitbox bound to that interface AND is intersecting.
        foreach (var myHitbox in hitboxes)
        {
            if (!myHitbox.IsTargetingInterface<T>()) continue;
            foreach (var hitbox in col.hitboxes)
            {
                if (hitbox.IsBoundToInterface<T>() && myHitbox.Intersects(hitbox))
                    return true;
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
}