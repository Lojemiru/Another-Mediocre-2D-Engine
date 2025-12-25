namespace AM2E.Collision;

public abstract class RectangleHitboxBase : Hitbox
{
    public int Width { get; protected set; }
    public int Height { get; protected set; }

    public sealed override int BoundLeft => X - OriginX;

    public sealed override int BoundRight => BoundLeft + Width - 1;

    public sealed override int BoundTop => Y - OriginY;

    public sealed override int BoundBottom => BoundTop + Height - 1;

    private protected RectangleHitboxBase(int w, int h, int originX = 0, int originY = 0)
    {
        Width = w;
        Height = h;
        OriginX = InitialOriginX = originX;
        OriginY = InitialOriginY = originY;
    }

    public sealed override void ApplyFlips(bool xFlip, bool yFlip)
    {
        base.ApplyFlips(xFlip, yFlip);
        OriginX = FlippedX ? (Width - 1) - InitialOriginX : InitialOriginX;
        OriginY = FlippedY ? (Height - 1) - InitialOriginY : InitialOriginY;
        Collider?.SyncBounds();
    }

    public sealed override void UpdateOrigin(int x, int y)
    {
        InitialOriginX = x;
        InitialOriginY = y;
        OriginX = FlippedX ? (Width - 1) - InitialOriginX : InitialOriginX;
        OriginY = FlippedY ? (Height - 1) - InitialOriginY : InitialOriginY;
        Collider?.SyncBounds();
    }

    public override bool IntersectsLine(int x1, int y1, int x2, int y2)
    {
        // If we contain an endpoint of the line segment, it obviously intersects.
        if (ContainsPoint(x1, y1) || ContainsPoint(x2, y2))
            return true;

        // Otherwise, the given line segment MUST intersect one of our diagonals if it is intersecting the rectangle.
        return MathHelper.DoLinesIntersect(x1, y1, x2, y2, BoundLeft, BoundTop, BoundRight + 0.9f, BoundBottom + 0.9f) ||
               MathHelper.DoLinesIntersect(x1, y1, x2, y2, BoundLeft, BoundBottom + 0.9f, BoundRight + 0.9f, BoundTop);
    }
}