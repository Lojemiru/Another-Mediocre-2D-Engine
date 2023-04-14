using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Point = System.Drawing.Point;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace AM2E.Collision;

public abstract class PolygonHitbox : Hitbox
{
    private readonly Point[] untranslatedPoints;
    private readonly Point[] points;

    private int furthestLeft;
    private int furthestRight;
    private int furthestTop;
    private int furthestBottom;
    public float Angle { get; private set; } = 0;

    public sealed override int BoundLeft => X + furthestLeft;

    public sealed override int BoundRight => X + furthestRight;

    public sealed override int BoundTop => Y + furthestTop;

    public sealed override int BoundBottom => Y + furthestBottom;

    protected PolygonHitbox(int size, int x, int y, int offsetX = 0, int offsetY = 0)
    {
        X = x;
        Y = y;
        points = new Point[size];
        untranslatedPoints = new Point[size];
        OffsetX = offsetX;
        OffsetY = offsetY;
    }
    
    private protected void SetPoint(int index, int x, int y)
    {
        x -= OffsetX;
        y -= OffsetY;
        points[index] = new Point(x, y);
        untranslatedPoints[index] = new Point(x, y);
    }

    public void ApplyRotation(float angle)
    {
        Angle = angle % 360;

        var radAngle = Angle * Math.PI / 180;

        for (var i = 0; i < points.Length; i++)
        {
            var x = untranslatedPoints[i].X;
            var y = untranslatedPoints[i].Y;

            var distance = Math.Round(MathHelper.PointDistance(0f, 0, x, y));
        
            var originalAngle = MathHelper.PointAngle(0, 0, x, y);

            var cos = (float)Math.Cos(originalAngle + radAngle);
            var sin = (float)Math.Sin(originalAngle + radAngle);

            points[i] = new Point((int)(cos * distance), (int)(sin * distance));
        }
        
        RecalculateBounds();
    }

    private protected void RecalculateBounds()
    {
        furthestTop = points[0].Y;
        furthestBottom = furthestTop;
        furthestLeft = points[0].X;
        furthestRight = furthestLeft;
        
        for (var i = 1; i < points.Length; i++)
        {
            if (points[i].X < furthestLeft)
                furthestLeft = points[i].X;
            else if (points[i].X > furthestRight)
                furthestRight = points[i].X;

            if (points[i].Y < furthestTop)
                furthestTop = points[i].Y;
            else if (points[i].Y > furthestBottom)
                furthestBottom = points[i].Y;
        }
    }

    private static bool IsLeft(Point lineStart, Point lineEnd, int targetX, int targetY)
    {
        // hail stackoverflow: https://gamedev.stackexchange.com/questions/110229/how-do-i-efficiently-check-if-a-point-is-inside-a-rotated-rectangle
        return ((lineEnd.X - lineStart.X) * (targetY - lineStart.Y) - (targetX - lineStart.X) * (lineEnd.Y - lineStart.Y)) >= 0;
    }
    
    public override bool Intersects(RectangleHitbox hitbox)
    {
        // If we're not intersecting on bounds, exit early.
        if (!IntersectsBounds(hitbox))
            return false;

        // Check if any of our vertices are contained in the rectangle.
        foreach (var point in points)
        {
            if (hitbox.ContainsPoint(point.X, point.Y))
                return true;
        }
        
        // Check if either of the rectangle's diagonals intersects us.
        return IntersectsLine(hitbox.BoundLeft, hitbox.BoundTop, hitbox.BoundRight, hitbox.BoundBottom) ||
               IntersectsLine(hitbox.BoundRight, hitbox.BoundTop, hitbox.BoundLeft, hitbox.BoundBottom);
    }

    public override bool Intersects(CircleHitbox hitbox)
    {
        throw new NotImplementedException();
    }

    // Defer to PreciseHitbox.
    public override bool Intersects(PreciseHitbox hitbox) => hitbox.Intersects(this);

    public override bool Intersects(PolygonHitbox hitbox)
    {
        /*
         * THE SEPARATING AXIS THEOREM IS EXCEEDINGLY OVERKILL.
         * Unless I'm completely misunderstanding the speed at which you can perform polygon transformations.
         *
         * Instead, we can follow these three simple steps:
         * 1.) Check if we contain any vertex from the other polygon.
         * 2.) Check if the other polygon contains any of our vertices.
         * 3.) Check if any of our INNER line segments intersect with any of the other polygon's INNER line segments.
         *
         * This should guarantee that we are quickly able to determine collisions.
         * If a vertex is in one of the two polygons, you're obviously colliding.
         * Otherwise, a line must be going through the polygon or no collisions are occurring.
         * If we check against inner line segments equal to half the number of sides in each polygon,
         * we can guarantee that such lines must intersect if the polygons are colliding but do not contain any vertices.
         *
         * I'm calling this Snowflake Theorem. It probably already has a name, but I can't find it so... cope.
         */

        // If we're not intersecting on bounds, exit early.
        if (!IntersectsBounds(hitbox))
            return false;

        // Check if any of our diagonals intersect the other polygon.
        var len = points.Length / 2;
        for (var i = 0; i < len; i++)
        {
            var point = points[i];
            var opposite = points[MathHelper.Wrap(i + len, 0, points.Length)];
            if (hitbox.IntersectsLine(X + point.X, Y + point.Y, X + opposite.X, Y +opposite.Y))
                return true;
        }
        
        // Check all of the target's vertices against our vertices.
        // If we've got here, we only need to check endpoints and not the whole line, so this is the fastest option...
        foreach (var point in hitbox.points)
        {
            if (ContainsPoint(hitbox.X + point.X, hitbox.Y + point.Y))
                return true;
        }

        return false;
    }

    public bool IntersectsLine(int x1, int y1, int x2, int y2)
    {
        if (ContainsPoint(x1, y1) || ContainsPoint(x2, y2))
            return true;

        var len = points.Length / 2;

        for (var i = 0; i < len; i++)
        {
            var point = points[i];
            var opposite = points[MathHelper.Wrap(i + len, 0, points.Length)];
            if (MathHelper.DoLinesIntersect(x1 - X, y1 - Y, x2 - X, y2 - Y, point.X, point.Y, opposite.X, opposite.Y))
                return true;
        }

        return false;
    }

    public override bool ContainsPoint(int x, int y)
    {
        if (!ContainsPointInBounds(x, y))
            return false;

        x -= X;
        y -= Y;
        
        for (var i = 0; i < points.Length; i++)
        {
            var end = i < (points.Length - 1) ? i + 1 : 0;
            if (!IsLeft(points[i], points[end], x, y))
                return false;
        }

        return true;
    }
    
    // TODO: Finish moving all the debug render shenanigans to the parent class
    private static Vector2 position = new();
    private static Vector2 origin = new();
    private static Vector2 scale = new(0, 1);
    
    public void Draw(SpriteBatch spriteBatch)
    {
        position.X = X;
        position.Y = Y;
        spriteBatch.Draw(Pixel, position, Color.Lime);
        
        for (var i = 0; i < points.Length; i++)
        {
            var next = points[i < (points.Length - 1) ? i + 1 : 0];
            position.X = points[i].X + X;
            position.Y = points[i].Y + Y;
            scale.X = (float)Math.Round(MathHelper.PointDistance(points[i].X, points[i].Y, next.X, next.Y));
            var rotation = (float)(Math.Atan2(next.Y - points[i].Y, next.X - points[i].X));
            spriteBatch.Draw(Pixel, position, null, Color, rotation, origin, scale, SpriteEffects.None, 0);
            
            next = untranslatedPoints[i < (points.Length - 1) ? i + 1 : 0];
            position.X = untranslatedPoints[i].X + X;
            position.Y = untranslatedPoints[i].Y + Y;
            scale.X = (float)Math.Round(MathHelper.PointDistance(untranslatedPoints[i].X, untranslatedPoints[i].Y, next.X, next.Y));
            rotation = (float)(Math.Atan2(next.Y - untranslatedPoints[i].Y, next.X - untranslatedPoints[i].X));
            spriteBatch.Draw(Pixel, position, null, Color * 0.2f, rotation, origin, scale, SpriteEffects.None, 0);
        }

        position.X = BoundLeft;
        position.Y = BoundTop;
        spriteBatch.Draw(Pixel, position, Color.Orange);

        position.X = BoundRight;
        spriteBatch.Draw(Pixel, position, Color.Orange);
        
        position.Y = BoundBottom;
        spriteBatch.Draw(Pixel, position, Color.Orange);
        
        position.X = BoundLeft;
        spriteBatch.Draw(Pixel, position, Color.Orange);
    }
}