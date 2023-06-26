namespace AM2E.Collision;

public abstract class RectangleHitboxBase : Hitbox
{
    public int Width { get; protected set; }
    public int Height { get; protected set; }

    public sealed override int BoundLeft => X - OffsetX;

    public sealed override int BoundRight => BoundLeft + Width - 1;

    public sealed override int BoundTop => Y - OffsetY;

    public sealed override int BoundBottom => BoundTop + Height - 1;

    private protected RectangleHitboxBase(int x, int y, int w, int h, int offsetX = 0, int offsetY = 0)
    {
        X = x;
        Y = y;
        Width = w;
        Height = h;
        OffsetX = InitialOffsetX = offsetX;
        OffsetY = InitialOffsetY = offsetY;
    }

    public sealed override void ApplyFlips(bool xFlip, bool yFlip)
    {
        base.ApplyFlips(xFlip, yFlip);
        OffsetX = FlippedX ? (Width - 1) - InitialOffsetX : InitialOffsetX;
        OffsetY = FlippedY ? (Height - 1) - InitialOffsetY : InitialOffsetY;
    }
    
    public override bool IntersectsLine(int x1, int y1, int x2, int y2)
    {
        // If we contain an endpoint of the line segment, it obviously intersects.
        if (ContainsPoint(x1, y1) || ContainsPoint(x2, y2))
            return true;

        // Otherwise, the given line segment MUST intersect one of our diagonals if it is intersecting the rectangle.
        return MathHelper.DoLinesIntersect(x1, y1, x2, y2, BoundLeft, BoundTop, BoundRight, BoundBottom) ||
               MathHelper.DoLinesIntersect(x1, y1, x2, y2, BoundLeft, BoundBottom, BoundRight, BoundTop);
    }
}