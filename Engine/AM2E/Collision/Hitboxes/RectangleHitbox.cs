using Microsoft.Xna.Framework;

namespace AM2E.Collision;

public class RectangleHitbox : Hitbox
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public override int BoundLeft => X - OffsetX;

    public override int BoundRight => BoundLeft + Width - 1;

    public override int BoundTop => Y - OffsetY;

    public override int BoundBottom => BoundTop + Height - 1;

    public RectangleHitbox(int x, int y, int w, int h, int offsetX = 0, int offsetY = 0)
    {
        X = x;
        Y = y;
        Width = w;
        Height = h;
        OffsetX = InitialOffsetX = offsetX;
        OffsetY = InitialOffsetY = offsetY;
    }

    public void Resize(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public override void ApplyFlips(bool xFlip, bool yFlip)
    {
        base.ApplyFlips(xFlip, yFlip);
        OffsetX = FlippedX ? (Width - 1) - InitialOffsetX : InitialOffsetX;
        OffsetY = FlippedY ? (Height - 1) - InitialOffsetY : InitialOffsetY;
    }

    protected override bool Intersects(RectangleHitbox hitbox)
    {
        return !(BoundRight < hitbox.BoundLeft || hitbox.BoundRight < BoundLeft || BoundBottom < hitbox.BoundTop || hitbox.BoundBottom < BoundTop);
    }

    protected override bool Intersects(CircleHitbox hitbox)
    {
        // Only two conditions: circle center is in rectangle, or endpoint of radius is in rectangle
        if (ContainsPoint(hitbox.X, hitbox.Y)) return true;

        var centerX = (float)(BoundLeft + BoundRight) / 2;
        var centerY = (float)(BoundTop + BoundBottom) / 2;

        var pos = new Vector2(centerX - hitbox.X, centerY - hitbox.Y);
        
        if ((int)pos.Length() > hitbox.Radius) 
        {
            pos.Normalize();
            pos *= hitbox.Radius;
        }

        return ContainsPoint((int)(hitbox.X - pos.X), (int)(hitbox.Y - pos.Y));
    }

    protected override bool Intersects(PreciseHitbox hitbox)
    {
        return hitbox.Intersects(this);
    }

    public override bool ContainsPoint(int x, int y)
    {
        return !(x < BoundLeft || x > BoundRight || y < BoundTop || y > BoundBottom);
    }
}