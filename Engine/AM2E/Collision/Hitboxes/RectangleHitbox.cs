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

    // TODO: Defer to circle instead?
    public override bool Intersects(CircleHitbox hitbox)
    {
        // Only two conditions: circle center is in rectangle, or endpoint of radius is in rectangle
        if (ContainsPoint(hitbox.X, hitbox.Y)) return true;

        var centerX = (float)(BoundLeft + BoundRight) / 2;
        var centerY = (float)(BoundTop + BoundBottom) / 2;

        var pos = new Vector2(centerX - hitbox.X, centerY - hitbox.Y);
        
        // ReSharper disable once InvertIf
        if ((int)pos.Length() > hitbox.Radius) 
        {
            pos.Normalize();
            pos *= hitbox.Radius;
        }

        return ContainsPoint((int)(hitbox.X - pos.X), (int)(hitbox.Y - pos.Y));
    }

    // Defer to PreciseHitbox.
    public override bool Intersects(PreciseHitbox hitbox) => hitbox.Intersects(this);

    // Defer to PolygonHitbox.
    public override bool Intersects(PolygonHitbox hitbox) => hitbox.Intersects(this);

    // Defer to whether or not the point is in bounds.
    public override bool ContainsPoint(int x, int y) => ContainsPointInBounds(x, y);

    public bool IntersectsLine(int x1, int y1, int x2, int y2)
    {
        // If we contain an endpoint of the line segment, it obviously intersects.
        if (ContainsPoint(x1, y1) || ContainsPoint(x2, y2))
            return true;

        // Otherwise, the given line segment MUST intersect one of our diagonals if it is intersecting the rectangle.
        return MathHelper.DoLinesIntersect(x1, y1, x2, y2, BoundLeft, BoundTop, BoundRight, BoundBottom) ||
               MathHelper.DoLinesIntersect(x1, y1, x2, y2, BoundLeft, BoundBottom, BoundRight, BoundTop);
    }
}