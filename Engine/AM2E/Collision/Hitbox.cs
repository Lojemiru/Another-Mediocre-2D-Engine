using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

namespace AM2E.Collision;

// TODO: Implement rotating rectangles :)

public abstract class Hitbox
{
    private class TypeContainer<T> where T : ICollider
    {}
    public int OffsetX { get; protected set; } = 0;
    public int OffsetY { get; protected set; } = 0;
    protected int InitialOffsetX = 0;
    protected int InitialOffsetY = 0;
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;

    public abstract int BoundLeft { get; }
    public abstract int BoundRight { get; }
    public abstract int BoundTop { get; }
    public abstract int BoundBottom { get; }

    public bool FlippedX { get; private set; } = false;
    public bool FlippedY { get; private set; } = false;
        
    public virtual void ApplyFlips(bool xFlip, bool yFlip)
    {
        FlippedX = xFlip;
        FlippedY = yFlip;
    }
        
    private List<object> boundInterfaces;
    private List<object> targetInterfaces;

    public bool IsBoundToInterface<T>() where T : ICollider
    {
        if (boundInterfaces == null)
            return true;
            
        foreach (var x in boundInterfaces)
        {
            if (x is TypeContainer<T>)
                return true;
        }

        return false;
    }

    public void BindToInterface<T>() where T : ICollider
    {
        boundInterfaces ??= new List<object>();
        boundInterfaces.Add(new TypeContainer<T>());
    }

    public bool IsTargetingInterface<T>() where T : ICollider
    {
        if (targetInterfaces == null) 
            return true;
            
        foreach (var x in targetInterfaces)
        {
            if (x is TypeContainer<T>)
                return true;
        }

        return false;
    }
        
    public void TargetInterface<T>() where T : ICollider
    {
        targetInterfaces ??= new List<object>();
        targetInterfaces.Add(new TypeContainer<T>());
    }

    protected abstract bool Intersects(RectangleHitbox hitbox);
    protected abstract bool Intersects(CircleHitbox hitbox);
    protected abstract bool Intersects(PreciseHitbox hitbox);
    public abstract bool ContainsPoint(int x, int y);
    public bool Intersects(Hitbox hitbox)
    {
        return hitbox switch
        {
            // Check Precise first so that we don't accidentally grab them as Rectangles :D
            PreciseHitbox preciseHitbox => Intersects(preciseHitbox),
            RectangleHitbox rectangleHitbox => Intersects(rectangleHitbox),
            CircleHitbox circleHitbox => Intersects(circleHitbox),
            _ => throw new ArgumentException("Hitbox type " + hitbox.GetType() + " is not a valid intersection target!")
        };
    }
}