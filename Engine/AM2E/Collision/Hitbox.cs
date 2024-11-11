using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

namespace AM2E.Collision;

public abstract class Hitbox
{
    protected static Vector2 DrawPosition = new();
    private protected static readonly Texture2D Pixel = new(EngineCore._graphics.GraphicsDevice, 1, 1);
    static Hitbox() 
        => Pixel.SetData(new[] { Color.White });
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

    public string Tag = "";
    
    // This SHOULD disallow publicly inheriting from this class. I think. Sorry guys, no custom Hitboxes :P
    private protected Hitbox() { }
        
    public virtual void ApplyFlips(bool xFlip, bool yFlip)
    {
        FlippedX = xFlip;
        FlippedY = yFlip;
    }
        
    private List<Type> boundInterfaces;
    private List<Type> targetInterfaces;

    private bool isBound = true;
    private bool doTarget = true;

    public bool IsBoundToInterface<T>() where T : ICollider
    {
        if (!isBound)
            return false;
        
        if (boundInterfaces == null)
            return true;

        foreach (var x in boundInterfaces)
        {
            if (typeof(T).IsAssignableFrom(x))
                return true;
        }

        return false;
    }

    public void BindToNothing()
    {
        isBound = false;
    }
    
    public void BindToInterface<T>() where T : ICollider
    {
        isBound = true;
        boundInterfaces ??= new List<Type>();
        boundInterfaces.Add(typeof(T));
    }

    public void BindToInterfaces(params Type[] types)
    {
        isBound = true;
        boundInterfaces ??= new List<Type>();
        foreach (var type in types)
            boundInterfaces.Add(type);
    }

    public bool IsTargetingInterface<T>() where T : ICollider
    {
        if (!doTarget)
            return false;
        
        if (targetInterfaces == null) 
            return true;
            
        foreach (var x in targetInterfaces)
        {
            if (typeof(T).IsAssignableFrom(x))
                return true;
        }

        return false;
    }

    public void TargetNothing()
    {
        doTarget = false;
    }
        
    public void TargetInterface<T>() where T : ICollider
    {
        doTarget = true;
        targetInterfaces ??= new List<Type>();
        targetInterfaces.Add(typeof(T));
    }

    public void TargetInterfaces(params Type[] types)
    {
        doTarget = true;
        targetInterfaces ??= new List<Type>();
        foreach (var type in types)
            targetInterfaces.Add(type);
    }

    public abstract bool Intersects(RectangleHitbox hitbox);
    public abstract bool Intersects(CircleHitbox hitbox);
    public abstract bool Intersects(PreciseHitbox hitbox);
    public abstract bool Intersects(PolygonHitbox hitbox);
    public abstract bool ContainsPoint(int x, int y);
    public abstract bool IntersectsLine(int x1, int y1, int x2, int y2);
    public bool Intersects(Hitbox hitbox)
    {
        return hitbox switch
        {
            // Check Precise first so that we don't accidentally grab them as Rectangles :D
            PreciseHitbox preciseHitbox => Intersects(preciseHitbox),
            RectangleHitbox rectangleHitbox => Intersects(rectangleHitbox),
            CircleHitbox circleHitbox => Intersects(circleHitbox),
            PolygonHitbox polyHitbox => Intersects(polyHitbox),
            _ => throw new ArgumentException("Hitbox type " + hitbox.GetType() + " is not a valid intersection target!")
        };
    }

    protected bool IntersectsBounds(Hitbox hitbox)
    {
        return !(BoundRight < hitbox.BoundLeft || hitbox.BoundRight < BoundLeft || BoundBottom < hitbox.BoundTop ||
                 hitbox.BoundBottom < BoundTop);
    }

    protected bool ContainsPointInBounds(int x, int y)
    {
        return !(x < BoundLeft || x > BoundRight || y < BoundTop || y > BoundBottom);
    }

    /// <summary>
    /// Debug hitbox render. This can be quite slow for various <see cref="Hitbox"/> children; this is intended ONLY for debugging use!
    /// </summary>
    /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to which draw calls will be submitted.</param>
    /// <param name="color">The <see cref="Color"/> that should be used for the base render.</param>
    public abstract void DebugRender(SpriteBatch spriteBatch, Color color = default);
}