using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Collision;

public sealed class CircleHitbox : Hitbox
{
    public int Radius { get; private set; }

    public override int BoundLeft 
        => X - OffsetX - Radius;

    public override int BoundRight 
        => X - OffsetX + Radius;

    public override int BoundTop 
        => Y - OffsetY - Radius;

    public override int BoundBottom 
        => Y - OffsetY + Radius;

    public CircleHitbox(int x, int y, int radius, int offsetX = 0, int offsetY = 0)
    {
        X = x;
        Y = y;
        Radius = radius;
        OffsetX = offsetX;
        OffsetY = offsetY;
    }

    public void Resize(int radius)
    {
        Radius = radius;
    }
    
    // Defer to RectangleHitbox, check has more to do with the rectangle.
    public override bool Intersects(RectangleHitbox hitbox) 
        => hitbox.Intersects(this);

    public override bool Intersects(CircleHitbox hitbox)
    {
        // Add radii, compare to distance between both centers
        return (Radius + hitbox.Radius + 1) > MathHelper.PointDistance(X - OffsetX, Y - OffsetY, hitbox.X - hitbox.OffsetX, hitbox.Y - hitbox.OffsetY);
    }

    // Defer to PreciseHitbox.
    public override bool Intersects(PreciseHitbox hitbox) 
        => hitbox.Intersects(this);

    // Defer to PolygonHitbox.
    public override bool Intersects(PolygonHitbox hitbox) 
        => hitbox.Intersects(this);

    public override bool ContainsPoint(int x, int y)
    {
        return ContainsPointInBounds(x, y) && (MathHelper.PointDistance(X - OffsetX, Y - OffsetY, x, y) - Radius < 0.5f);
    }

    private const float PI_HALVES = (float)Math.PI / 2;

    public override bool IntersectsLine(int x1, int y1, int x2, int y2)
    {
        // If we contain an endpoint, we're intersecting.
        if (ContainsPoint(x1, y1) || ContainsPoint(x2, y2))
            return true;

        // Get an angle that is perpendicular to the line we're checking...
        var angle = MathHelper.PointAngle(x1, y1, x2, y2) + PI_HALVES;

        // Get the X/Y components of that angle with our radius...
        var x = MathHelper.LineComponentX(angle, Radius + 0.5f);
        var y = MathHelper.LineComponentY(angle, Radius + 0.5f);

        // And return whether or not our perpendicular diameter and the input line intersect.
        return MathHelper.DoLinesIntersect(X - OffsetX - x, Y - OffsetY - y, X - OffsetX + x, Y - OffsetY + y, x1, y1, x2, y2);
    }
    
    public override void DebugRender(SpriteBatch spriteBatch, Color color = default)
    {
        if (color == default)
            color = Color.White;
        
        for (var i = 0; i < (BoundRight - BoundLeft) + 1; i++)
        {
            for (var j = 0; j < (BoundBottom - BoundTop) + 1; j++)
            {
                DrawPosition.X = BoundLeft + i;
                DrawPosition.Y = BoundTop + j;
                if (ContainsPoint(BoundLeft + i, BoundTop + j))
                    spriteBatch.Draw(Pixel, DrawPosition, color);
            }
        }
        
        spriteBatch.Draw(Pixel, new Vector2(X - OffsetX, Y - OffsetY), Color.Lime);
    }
}