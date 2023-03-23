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
}