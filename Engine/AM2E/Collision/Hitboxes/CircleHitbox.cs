namespace AM2E.Collision;

public class CircleHitbox : Hitbox
{
    // TODO: Actually finish circle -> other hitbox interactions
    // TODO: Constructor
    public int Radius { get; }

    public override int BoundLeft => X - Radius;

    public override int BoundRight => X + Radius;

    public override int BoundTop => Y - Radius;

    public override int BoundBottom => Y + Radius;

    protected override bool Intersects(RectangleHitbox hitbox)
    {
        return hitbox.Intersects(this);
    }

    protected override bool Intersects(CircleHitbox hitbox)
    {
        // Add radii, compare to distance between both centers
        return (Radius + hitbox.Radius) >= MathHelper.PointDistance(X, Y, hitbox.X, hitbox.Y);
    }

    protected override bool Intersects(PreciseHitbox hitbox)
    {
        return hitbox.Intersects(this);
    }

    public override bool ContainsPoint(int x, int y)
    {
        return MathHelper.PointDistance(X, Y, x, y) <= Radius;
    }
}