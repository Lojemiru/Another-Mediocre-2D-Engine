using System;

namespace AM2E.Collision;

// Inherits from RectangleHitbox for the bounds getters
public class PreciseHitbox : RectangleHitbox
{
    public bool[, ] Mask { get; }

    // TODO: Alt constructor that turns an image into the mask, or do we process that somewhere above?
    public PreciseHitbox(int x, int y, bool[, ] mask, int offsetX = 0, int offsetY = 0) : base(x, y, 
        mask.GetLength(0), mask.GetLength(1), offsetX, offsetY)
    {
        Mask = mask;
    }

    protected override bool Intersects(RectangleHitbox hitbox)
    {
        if (!base.Intersects(hitbox))
            return false;
            
        var startX = Math.Clamp(hitbox.BoundLeft - BoundLeft, 0, Width - 1);
        var startY = Math.Clamp(hitbox.BoundTop - BoundTop, 0, Height - 1);
        var endX = Math.Clamp(hitbox.BoundRight - BoundLeft + 1, 0, Width);
        var endY = Math.Clamp(hitbox.BoundBottom - BoundTop + 1, 0, Height);

        // TODO: Should this check be interlaced? Might speed up some use cases, but wouldn't affect our worst-case scenario :/
        for (var i = startX; i < endX; ++i)
        {
            for (var j = startY; j < endY; ++j)
            {
                if (CheckPointInMask(i, j))
                    return true;
            }
        }
            
        return false;
    }

    protected override bool Intersects(CircleHitbox hitbox)
    {
        // Early exit - return false if bounds don't even overlap
        // TODO: Turn this into a shared/more generic check kthx :)
        if (BoundRight < hitbox.BoundLeft || hitbox.BoundRight < BoundLeft || BoundBottom < hitbox.BoundTop || hitbox.BoundBottom < BoundTop)
            return false;

        var startX = Math.Clamp(hitbox.BoundLeft, 0, Width - 1);
        var startY = Math.Clamp(hitbox.BoundTop, 0, Height - 1);
        var endX = Math.Clamp(hitbox.BoundRight, 0, Width);
        var endY = Math.Clamp(hitbox.BoundBottom, 0, Height);

        for (var i = startX; i < endX; ++i)
        {
            for (var j = startY; j < endY; ++j)
            {
                if (CheckPointInMask(i, j) && hitbox.ContainsPoint(i, j)) 
                    return true;
            }
        }

        return false;
    }

    protected override bool Intersects(PreciseHitbox hitbox)
    {
        // Early exit - return false if bounds don't even overlap
        return !base.Intersects((RectangleHitbox)hitbox) &&
               // TODO: Based on rectangle -> precise check testing, this might be slightly scuffed. Give it a proper test.
               MaskIntersects(hitbox, hitbox.BoundLeft - BoundLeft, hitbox.BoundTop - BoundTop);
    }

    public override bool ContainsPoint(int x, int y)
    {
        // Check base, then return value of array cell
        return base.ContainsPoint(x, y) && CheckPointInMask(x - (X - OffsetX), y - (Y - OffsetY));
    }
        
    private bool CheckPointInMask(int x, int y)
    {
        return Mask[FlippedX ? (Width - 1) - x : x, FlippedY ? (Height - 1) - y : y];
    }

    public bool MaskIntersects(PreciseHitbox hitbox, int offsetX, int offsetY)
    {
        var startX = Math.Clamp(offsetX, 0, Width - 1);
        var startY = Math.Clamp(offsetY, 0, Height - 1);
        var endX = Math.Clamp(offsetX + hitbox.Mask.GetLength(0) - 1, 0, Width);
        var endY = Math.Clamp(offsetY + hitbox.Mask.GetLength(1) - 1, 0, Height);

        for (var i = startX; i < endX; ++i)
        {
            for (var j = startY; j < endY; ++j)
            {
                if (CheckPointInMask(i, j) && hitbox.CheckPointInMask(i, j))
                    return true;
            }
        }

        return false;
    }
}