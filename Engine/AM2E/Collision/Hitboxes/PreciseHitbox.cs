using System;
using System.Security.Cryptography;

namespace AM2E.Collision;

#region Design Notes

/*
 * Hoo boy, pixel-perfect collision masks. These things tend to be horribly slow, but they can be quite useful... I'd
 *      generally recommend /not/ using them if you can avoid it, but they're supported.
 *
 * We always run general bound checks before doing anything more complicated. This ensures that we're not wasting time
 *      on for() loops when we can quickly verify that we'd never collide with the other hitbox in the first place.
 *
 * We also trim down the space we check within the hitbox mask to just the overlap of the two hitboxes we're checking.
 *      Sure, the Math.Clamp() calls probably eat some extra clock cycles but it'll save us a ton of time considering
 *      that most collisions will occur with only a slight hitbox overlap.
 *
 * I considered interlacing the mask checks to hopefully find a collision quicker, but the extra logic required would
 *      probably end up creating more overhead in most situations compared to just doing the full loop.
 */

#endregion

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
    public override bool Intersects(CircleHitbox hitbox) 
        => IntersectsGeneric(hitbox);
    
    // Defer to generic check.
    public override bool Intersects(PreciseHitbox hitbox)
        => IntersectsGeneric(hitbox);

    // Defer to generic check.
    public override bool Intersects(PolygonHitbox hitbox) 
        => IntersectsGeneric(hitbox);

    public override bool IntersectsLine(int x1, int y1, int x2, int y2)
    {
        // Obviously, we want to make sure our rectangle bounds intersect first...
        if (!base.IntersectsLine(x1, y1, x2, y2))
            return false;

        // If we contain an endpoint, we can very simply exit.
        if (ContainsPoint(x1, y1) || ContainsPoint(x2, y2))
            return true;

        // Invert our point placement depending on distance to the relevant boundary.
        if (Math.Abs(x2 - BoundRight) < Math.Abs(x1 - BoundLeft) || Math.Abs(y2 - BoundBottom) < Math.Abs(y1 - BoundTop))
        {
            (x1, x2) = (x2, x1);
            (y1, y2) = (y2, y1);
        }

        // If we don't contain one of the endpoints, we should move the line so that we do to save on iteration cycles.
        // Except this doesn't ACTUALLY ensure we're in bounds, it just gets us closer. Close enough, I guess.
        // If this is slow later, consider tuning it up to do that more precisely.
        if (!ContainsPointInBounds(x1, y1))
        {
            // Get our angle; we'll use this to determine the amount to shift our sub-axis via trig.
            var angle = MathHelper.PointAngle(x1, y1, x2, y2);
            
            // Get the amounts we need to move to get to the nearest boundary from outside; this is 0 if we're inside.
            var amtX = Math.Clamp(x1, BoundLeft, BoundRight) - x1;
            var amtY = Math.Clamp(y1, BoundTop, BoundBottom) - y1;
            
            // If X is further from its bounds than Y...
            if (Math.Abs(amtX) > Math.Abs(amtY))
            {
                // Shift on X and adjust Y with trig to compensate.
                x1 += amtX;
                y1 += (int)Math.Round(Math.Tan(angle) * amtX, MidpointRounding.AwayFromZero);
            }
            // Otherwise...
            else
            {
                // Shift on Y and adjust X with trig to compensate.
                y1 += amtY;
                x1 += (int)Math.Round(amtY / Math.Tan(angle), MidpointRounding.AwayFromZero);
            }
        }
        
        // Following portion is from https://stackoverflow.com/a/11683720 with minor modifications.

        var w = x2 - x1;
        var h = y2 - y1;
        int dx1 = 0,
            dy1 = 0,
            dx2 = 0,
            dy2 = 0;
        
        if (w<0) 
            dx1 = -1;
        else if (w>0) 
            dx1 = 1;
        
        if (h<0) 
            dy1 = -1; 
        else if (h>0) 
            dy1 = 1;
        
        if (w<0) 
            dx2 = -1; 
        else if (w>0) 
            dx2 = 1;
        
        var longest = Math.Abs(w);
        var shortest = Math.Abs(h);
        
        if (!(longest > shortest))
        {
            longest = Math.Abs(h);
            shortest = Math.Abs(w);
            if (h<0) 
                dy2 = -1;
            else if (h>0) 
                dy2 = 1;
            dx2 = 0;
        }
        
        var numerator = longest >> 1;

        var lastIn = false;

        for (var i=0; i <= longest; i++)
        {
            if (lastIn && !ContainsPointInBounds(x1, y1))
                return false;

            lastIn = ContainsPointInBounds(x1, y1);

            if (x1 >= BoundLeft && y1 >= BoundTop && x1 <= BoundRight && y1 <= BoundBottom && CheckPointInMask(x1 - BoundLeft, y1 - BoundTop))
                return true;

            numerator += shortest;
            if (!(numerator < longest)) 
            {
                numerator -= longest;
                x1 += dx1;
                y1 += dy1;
            }
            else
            {
                x1 += dx2;
                y1 += dy2;
            }
        }

        return false;
    }

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