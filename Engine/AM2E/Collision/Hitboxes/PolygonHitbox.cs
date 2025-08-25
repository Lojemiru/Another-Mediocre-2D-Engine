using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Point = Microsoft.Xna.Framework.Point;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace AM2E.Collision;

public class PolygonHitbox : Hitbox
{
    // TODO: Scaling?
    
    private readonly Point[] untranslatedPoints;
    private readonly Point[] points;

    private int furthestLeft;
    private int furthestRight;
    private int furthestTop;
    private int furthestBottom;
    private const double TO_RADIANS = Math.PI / 180;
    public float Angle { get; private set; } = 0;

    public sealed override int BoundLeft 
        => X + furthestLeft;

    public sealed override int BoundRight 
        => X + furthestRight;

    public sealed override int BoundTop 
        => Y + furthestTop;

    public sealed override int BoundBottom 
        => Y + furthestBottom;

    public PolygonHitbox(IList<Point> points, int originX = 0, int originY = 0)
    {
        if (points.Count < 3)
            throw new ArgumentOutOfRangeException(nameof(points), "Points collection must be at least length 3!");
        
        this.points = new Point[points.Count];
        untranslatedPoints = new Point[points.Count];
        OriginX = originX;
        OriginY = originY;

        for (var i = 0; i < points.Count; i++)
            SetPoint(i, points[i].X, points[i].Y);
        
        RecalculateBounds();
    }

    protected PolygonHitbox(int size, int originX = 0, int originY = 0)
    {
        points = new Point[size];
        untranslatedPoints = new Point[size];
        OriginX = originX;
        OriginY = originY;
    }
    
    private protected void SetPoint(int index, int x, int y)
    {
        x -= OriginX;
        y -= OriginY;
        points[index] = new Point(x, y);
        untranslatedPoints[index] = new Point(x, y);
    }

    public void ApplyRotation(float angle)
    {
        Angle = angle % 360;
        ApplyTransform();
    }

    public override void ApplyOffset(int x, int y)
    {
        for (var i = 0; i < untranslatedPoints.Length; i++)
        {
            var point = untranslatedPoints[i];
            untranslatedPoints[i] = new Point(point.X + OriginX - x, point.Y + OriginY - y);
        }

        OriginX = x;
        OriginY = y;
        
        ApplyTransform();
    }

    private void ApplyTransform()
    {
        var radAngle = Angle * TO_RADIANS;
        var multX = FlippedX ? -1 : 1;
        var multY = FlippedY ? -1 : 1;

        for (var i = 0; i < points.Length; i++)
        {
            var x = untranslatedPoints[i].X * multX;
            var y = untranslatedPoints[i].Y * multY;

            var distance = Math.Round(MathHelper.PointDistance(0f, 0, x, y));
            var originalAngle = MathHelper.PointAngle(0, 0, x, y);

            points[i] = new Point((int)Math.Round(Math.Cos(originalAngle + radAngle) * distance), (int)Math.Round(Math.Sin(originalAngle + radAngle) * distance));
        }
        
        RecalculateBounds();
    }

    public override void ApplyFlips(bool xFlip, bool yFlip)
    {
        base.ApplyFlips(xFlip, yFlip);
        ApplyTransform();
    }

    private protected void RecalculateBounds()
    {
        furthestTop = furthestBottom = points[0].Y;
        furthestLeft = furthestRight = points[0].X;

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

    private bool IsLeft(Point lineStart, Point lineEnd, float targetX, float targetY)
    {
        // hail stackoverflow: https://gamedev.stackexchange.com/questions/110229/how-do-i-efficiently-check-if-a-point-is-inside-a-rotated-rectangle
        // ...and also use some XORing magic to make this function work regardless of our flips combination.
        return (lineEnd.X - lineStart.X) * (targetY - lineStart.Y) - (targetX - lineStart.X) * (lineEnd.Y - lineStart.Y) > 0 ^ FlippedX ^ FlippedY;
    }
    
    public override bool Intersects(RectangleHitbox hitbox)
    {
        // If we're not intersecting on bounds, exit early.
        if (!IntersectsBounds(hitbox))
            return false;

        // Check if any of our vertices are contained in the rectangle.
        foreach (var point in points)
        {
            if (hitbox.ContainsPoint(X + point.X, Y + point.Y))
                return true;
        }
        
        // Check if either of the rectangle's diagonals intersects us.
        return IntersectsLine(hitbox.BoundLeft, hitbox.BoundTop, hitbox.BoundRight, hitbox.BoundBottom) ||
               IntersectsLine(hitbox.BoundRight, hitbox.BoundTop, hitbox.BoundLeft, hitbox.BoundBottom);
    }

    public override bool Intersects(CircleHitbox hitbox)
    {
        // If we're not intersecting on bounds, exit early.
        if (!IntersectsBounds(hitbox))
            return false;
        
        // Either a) circle center is in polygon...
        if (ContainsPoint(hitbox.X - hitbox.OriginX, hitbox.Y - hitbox.OriginY))
            return true;

        // ...or b) an edge intersects.
        for (var i = 0; i < points.Length; i++)
        {
            var last = points[MathHelper.Wrap(i - 1, 0, points.Length)];
            var current = points[i];
            if (hitbox.IntersectsLine(X + last.X, Y + last.Y, X + current.X, Y + current.Y))
                return true;
        }
        
        return false;
    }

    // Defer to PreciseHitbox.
    public override bool Intersects(PreciseHitbox hitbox) 
        => hitbox.Intersects(this);

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
         * I'm calling this Polygon Spoke Theorem. It probably already has a name, but I can't find it so... cope.
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

    public override bool IntersectsLine(int x1, int y1, int x2, int y2)
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
    
    private static readonly Vector2 Origin = new();
    private static Vector2 scale = new(0, 1);
    
    public override void DebugRender(SpriteBatch spriteBatch, Color color = default)
    {
        if (color == default)
            color = Color.White;
        
        // Draw origin.
        spriteBatch.Draw(Pixel, new Vector2(X, Y), Color.Lime);
        
        // Draw lines.
        for (var i = 0; i < points.Length; i++)
        {
            var next = points[i < (points.Length - 1) ? i + 1 : 0];
            scale.X = (float)Math.Round(MathHelper.PointDistance(points[i].X, points[i].Y, next.X, next.Y));
            var rotation = (float)(Math.Atan2(next.Y - points[i].Y, next.X - points[i].X));
            spriteBatch.Draw(Pixel, new Vector2(points[i].X + X, points[i].Y + Y), null, color, rotation, Origin, scale, SpriteEffects.None, 0);
            
            next = untranslatedPoints[i < (points.Length - 1) ? i + 1 : 0];
            scale.X = (float)Math.Round(MathHelper.PointDistance(untranslatedPoints[i].X, untranslatedPoints[i].Y, next.X, next.Y));
            rotation = (float)(Math.Atan2(next.Y - untranslatedPoints[i].Y, next.X - untranslatedPoints[i].X));
            spriteBatch.Draw(Pixel, new Vector2(untranslatedPoints[i].X + X, untranslatedPoints[i].Y + Y), null, color * 0.2f, rotation, Origin, scale, SpriteEffects.None, 0);
        }
        
        // Draw bounding box corners.
        spriteBatch.Draw(Pixel, new Vector2(BoundLeft, BoundTop), Color.Orange);
        spriteBatch.Draw(Pixel, new Vector2(BoundRight, BoundTop), Color.Orange);
        spriteBatch.Draw(Pixel, new Vector2(BoundRight, BoundBottom), Color.Orange);
        spriteBatch.Draw(Pixel, new Vector2(BoundLeft, BoundBottom), Color.Orange);
    }
}