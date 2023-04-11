using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Point = System.Drawing.Point;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace AM2E.Collision;

public abstract class PolygonHitbox : Hitbox
{
    private Point[] untranslatedPoints;
    private Point[] points;

    private int furthestLeft;
    private int furthestRight;
    private int furthestTop;
    private int furthestBottom;

    public Color Color = Color.White;

    public sealed override int BoundLeft => X - OffsetX + furthestLeft;

    public sealed override int BoundRight => X - OffsetX + furthestRight;

    public sealed override int BoundTop => Y - OffsetY + furthestTop;

    public sealed override int BoundBottom => Y - OffsetY + furthestBottom;

    protected PolygonHitbox(int size, int x, int y, int offsetX = 0, int offsetY = 0)
    {
        X = x;
        Y = y;
        points = new Point[size];
        untranslatedPoints = new Point[size];
        OffsetX = offsetX;
        OffsetY = offsetY;
        
        pixel = new Texture2D(EngineCore._graphics.GraphicsDevice, 1, 1);
        pixel.SetData(new[] {Microsoft.Xna.Framework.Color.White});
    }
    
    #region DEBUG

    private Texture2D pixel;
    
    #endregion
    
    private protected void SetPoint(int index, int x, int y)
    {
        // TODO: This will break things if we're rotated and set this, but this should only be called during construction anyway. Better design pattern somewhere?
        points[index] = new Point(x, y);
        untranslatedPoints[index] = new Point(x, y);
    }

    public void ApplyRotation()
    {
        throw new NotImplementedException();
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

    private static bool IsLeft(Point lineStart, Point lineEnd, int targetX, int targetY) => 
        IsLeft(lineStart.X, lineStart.Y, lineEnd.X, lineEnd.Y, targetX, targetY);

    private static bool IsLeft(int x1, int y1, int x2, int y2, int x3, int y3)
    {
        // hail stackoverflow: https://gamedev.stackexchange.com/questions/110229/how-do-i-efficiently-check-if-a-point-is-inside-a-rotated-rectangle
        return ((x2 - x1) * (y3 - y1) - (x3 - x1) * (y2 - y1)) >= 0;
    }
    
    public override bool Intersects(RectangleHitbox hitbox)
    {
        // If we're not intersecting on bounds, exit early.
        if (!IntersectsBounds(hitbox))
            return false;
        
        // If we contain the center point, the rectangle MUST be intersecting.
        // This also accounts for situations where the rectangle is completely enclosed, not touching any edges.
        if (ContainsPoint((hitbox.BoundRight + hitbox.BoundLeft) / 2, (hitbox.BoundBottom + hitbox.BoundTop) / 2))
            return true;
        
        // Get universal point offsets.
        var additiveX = X - OffsetX;
        var additiveY = Y - OffsetY;
        
        for (var i = 0; i < points.Length; i++)
        {
            // Create line for this point and see if the rectangle intersects it.
            var next = points[i < (points.Length - 1) ? i + 1 : 0];
            if (hitbox.IntersectsLine(points[i].X + additiveX, points[i].Y + additiveY, next.X + additiveX, next.Y + additiveY))
                return true;
        }

        return false;
    }

    public override bool Intersects(CircleHitbox hitbox)
    {
        throw new NotImplementedException();
    }

    public override bool Intersects(PreciseHitbox hitbox)
    {
        return false;
        throw new NotImplementedException();
    }

    public override bool Intersects(PolygonHitbox hitbox)
    {
        // Check general bounds collision!
        if (!IntersectsBounds(hitbox))
            return false;
        
        // TODO: Test this!
        var additiveX = X - OffsetX;
        var additiveY = Y - OffsetY;
        var additive2X = hitbox.X - hitbox.OffsetX;
        var additive2Y = hitbox.Y - hitbox.OffsetY;

        // First, check for whether or not we contain an endpoint from the other polygon.
        foreach (var point in hitbox.points)
        {
            if (ContainsPoint(point.X, point.Y))
                return true;
        }

        for (var i = 0; i < points.Length; i++)
        {
            var x = points[i].X + additiveX;
            var y = points[i].Y + additiveY;
            
            // Then, do the reverse.
            if (hitbox.ContainsPoint(x, y))
                return true;
            
            var iNext = points[i < (points.Length - 1) ? i + 1 : 0];
            
            // If that didn't work, we enter the worst-case scenario and check each of our lines against each of the polygon's lines.
            for (var j = 0; j < hitbox.points.Length; j++)
            {
                var jNext = hitbox.points[j < (hitbox.points.Length - 1) ? j + 1 : 0];
                var x2 = hitbox.points[j].X + additive2X;
                var y2 = hitbox.points[j].Y + additive2Y;
                
                if (MathHelper.DoLinesIntersect(x, y, iNext.X + additiveX, iNext.Y + additiveY, 
                        x2, y2, jNext.X + additive2X, jNext.Y + additive2Y))
                    return true;
            }
        }

        return false;
    }

    public override bool ContainsPoint(int x, int y)
    {
        if (!ContainsPointInBounds(x, y))
            return false;

        x -= X - OffsetX;
        y -= Y - OffsetY;
        
        for (var i = 0; i < points.Length; i++)
        {
            var end = i < (points.Length - 1) ? i + 1 : 0;
            if (!IsLeft(points[i], points[end], x, y))
                return false;
        }

        return true;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // TODO: Remove debug? This could actually be pretty nice to keep around...
        var vec = new Vector2();
        var origin = new Vector2();
        for (var i = 0; i < points.Length; i++)
        {
            var next = points[i < (points.Length - 1) ? i + 1 : 0];
            vec.X = points[i].X + (X - OffsetX);
            vec.Y = points[i].Y + (Y - OffsetY);
            var rotation = (float)(Math.Atan2(next.Y - points[i].Y, next.X - points[i].X));
            spriteBatch.Draw(pixel, vec, null, Color, rotation, origin, new Vector2(MathHelper.PointDistance(points[i].X, points[i].Y, next.X, next.Y), 1), SpriteEffects.None, 0);
        }
    }
}