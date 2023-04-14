using System;

namespace AM2E.Collision;

public sealed class PreciseHitbox : RectangleHitboxBase
{
    public bool[, ] Mask { get; }

    public PreciseHitbox(int x, int y, bool[,] mask, int offsetX = 0, int offsetY = 0) : 
        base(x, y, mask.GetLength(0), mask.GetLength(1), offsetX, offsetY)
    {
        Mask = mask;
    }

    public override bool Intersects(RectangleHitbox hitbox)
    {
        if (!IntersectsBounds(hitbox))
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

    // Defer to generic check.
    public override bool Intersects(CircleHitbox hitbox) => IntersectsGeneric(hitbox);
    
    // TODO: Can't this be the generic intersection instead?
    public override bool Intersects(PreciseHitbox hitbox)
    {
        return !IntersectsBounds(hitbox) &&
               // Early exit - return false if bounds don't even overlap
               // TODO: Based on rectangle -> precise check testing, this might be slightly scuffed. Give it a proper test.
               MaskIntersects(hitbox, hitbox.BoundLeft - BoundLeft, hitbox.BoundTop - BoundTop);
    }

    // Defer to generic check.
    public override bool Intersects(PolygonHitbox hitbox) => IntersectsGeneric(hitbox);

    private bool IntersectsGeneric(Hitbox hitbox)
    {
        if (!IntersectsBounds(hitbox))
            return false;
        
        var startX = Math.Clamp(hitbox.BoundLeft - BoundLeft, 0, Width - 1);
        var startY = Math.Clamp(hitbox.BoundTop - BoundTop, 0, Height - 1);
        var endX = Math.Clamp(hitbox.BoundRight - BoundLeft + 1, 0, Width);
        var endY = Math.Clamp(hitbox.BoundBottom - BoundTop + 1, 0, Height);
        
        for (var i = startX; i < endX; ++i)
        {
            for (var j = startY; j < endY; ++j)
            {
                if (CheckPointInMask(i, j) && hitbox.ContainsPoint(X - OffsetX + i, Y - OffsetY + j))
                    return true;
            }
        }
        
        return false;
    }

    public override bool ContainsPoint(int x, int y)
    {
        // Check base, then return value of array cell
        return ContainsPointInBounds(x, y) && CheckPointInMask(x - (X - OffsetX), y - (Y - OffsetY));
    }
        
    private bool CheckPointInMask(int x, int y)
    {
        return Mask[FlippedX ? (Width - 1) - x : x, FlippedY ? (Height - 1) - y : y];
    }

    private bool MaskIntersects(PreciseHitbox hitbox, int offsetX, int offsetY)
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