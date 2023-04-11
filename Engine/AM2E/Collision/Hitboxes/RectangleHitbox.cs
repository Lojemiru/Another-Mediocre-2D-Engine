using Microsoft.Xna.Framework;

namespace AM2E.Collision;

public sealed class RectangleHitbox : RectangleHitboxBase
{
    public RectangleHitbox(int x, int y, int w, int h, int offsetX = 0, int offsetY = 0) : 
        base(x, y, w, h, offsetX, offsetY)
    {
    }
    
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
    
    public override bool Intersects(RectangleHitbox hitbox)
    {
        return IntersectsBounds(hitbox);
    }

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

    public override bool Intersects(PreciseHitbox hitbox)
    {
        return hitbox.Intersects(this);
    }

    public override bool Intersects(PolygonHitbox hitbox)
    {
        return hitbox.Intersects(this);
    }

    public override bool ContainsPoint(int x, int y)
    {
        return ContainsPointInBounds(x, y);
    }

    public bool IntersectsLine(int x1, int y1, int x2, int y2)
    {
        // Massive thanks to metamal at https://stackoverflow.com/questions/99353/how-to-test-if-a-line-segment-intersects-an-axis-aligned-rectange-in-2d
        // I am profoundly stupid when it comes to collisions that aren't perfectly axis-aligned
        
        // Find min and max X for the segment
        var minX = x1 > x2 ? x2 : x1;
        var maxX = x1 > x2 ? x1 : x2;

        // Find the intersection of the segment's and rectangle's x-projections

        if (maxX > BoundRight)
            maxX = BoundRight;

        if (minX < BoundLeft)
            minX = BoundLeft;
        
        // If their projections do not intersect return false
        if (minX > maxX)
            return false;

        // Find corresponding min and max Y for min and max X we found before
        var minY = y1;
        var maxY = y2;

        var dx = x2 - x1;

        if (dx != 0)
        {
            var a = (y2 - y1) / dx;
            var b = y1 - a * x1;
            minY = a * minX + b;
            maxY = a * maxX + b;
        }
        
        if (minY > maxY)
            (maxY, minY) = (minY, maxY);

        // Find the intersection of the segment's and rectangle's y-projections

        if (maxY > BoundBottom)
            maxY = BoundBottom;

        if (minY < BoundTop)
            minY = BoundTop;

        // If Y-projections do not intersect return false
        return !(minY > maxY); 
    }
}