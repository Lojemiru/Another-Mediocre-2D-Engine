using Microsoft.Xna.Framework;

namespace AM2E.Collision;

public sealed class RectangleHitbox : RectangleHitboxBase
{
    public RectangleHitbox(int x, int y, int w, int h, int offsetX = 0, int offsetY = 0) : 
        base(x, y, w, h, offsetX, offsetY) { }
    
    public void Resize(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public void Resize(int width, int height, int offsetX, int offsetY)
    {
        Resize(width, height);
        OffsetX = offsetX;
        OffsetY = offsetY;
    }

    // Defer to general bounds intersection.
    public override bool Intersects(RectangleHitbox hitbox) => IntersectsBounds(hitbox);
    
    public override bool Intersects(CircleHitbox hitbox)
    {
        if (!IntersectsBounds(hitbox))
            return false;
        
        // Only two conditions: circle center is in rectangle...
        if (ContainsPoint(hitbox.X, hitbox.Y)) 
            return true;

        // Or the circle intersects one of our edges.
        return hitbox.IntersectsLine(BoundLeft, BoundTop, BoundRight, BoundTop) || 
               hitbox.IntersectsLine(BoundRight, BoundTop, BoundRight, BoundBottom) || 
               hitbox.IntersectsLine(BoundLeft, BoundBottom, BoundRight, BoundBottom) ||
               hitbox.IntersectsLine(BoundLeft, BoundTop, BoundLeft, BoundBottom);
    }

    // Defer to PreciseHitbox.
    public override bool Intersects(PreciseHitbox hitbox) => hitbox.Intersects(this);

    // Defer to PolygonHitbox.
    public override bool Intersects(PolygonHitbox hitbox) => hitbox.Intersects(this);

    // Defer to whether or not the point is in bounds.
    public override bool ContainsPoint(int x, int y) => ContainsPointInBounds(x, y);
}