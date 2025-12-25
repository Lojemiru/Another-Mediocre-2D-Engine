using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Collision;

public sealed class RectangleHitbox : RectangleHitboxBase
{
    public RectangleHitbox(int w, int h, int originX = 0, int originY = 0) : 
        base(w, h, originX, originY) { }
    
    public void Resize(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public void Resize(int width, int height, int offsetX, int offsetY)
    {
        Resize(width, height);
        OriginX = offsetX;
        OriginY = offsetY;
        Collider?.SyncBounds();
    }

    // Defer to general bounds intersection.
    public override bool Intersects(RectangleHitbox hitbox) => IntersectsBounds(hitbox);
    
    public override bool Intersects(CircleHitbox hitbox)
    {
        if (!IntersectsBounds(hitbox))
            return false;
        
        // Only two conditions: circle center is in rectangle...
        if (ContainsPoint(hitbox.X - hitbox.OriginX, hitbox.Y - hitbox.OriginY)) 
            return true;

        // Or the circle intersects one of our edges.
        return hitbox.IntersectsLine(BoundLeft, BoundTop, BoundRight, BoundTop) || 
               hitbox.IntersectsLine(BoundRight, BoundTop, BoundRight, BoundBottom) || 
               hitbox.IntersectsLine(BoundLeft, BoundBottom, BoundRight, BoundBottom) ||
               hitbox.IntersectsLine(BoundLeft, BoundTop, BoundLeft, BoundBottom);
    }

    // Defer to PreciseHitbox.
    public override bool Intersects(PreciseHitbox hitbox) => hitbox.Intersects(this);

    // Defer to PolygonHitbox.
    public override bool Intersects(PolygonHitbox hitbox) => hitbox.Intersects(this);

    // Defer to whether or not the point is in bounds.
    public override bool ContainsPoint(int x, int y) => ContainsPointInBounds(x, y);

    private static Vector2 debugScale = new();
    private static Vector2 debugOffset = new();
    public override void DebugRender(SpriteBatch spriteBatch, Color color = default)
    {
        debugScale.X = Width;
        debugScale.Y = Height;
        debugOffset.X = 0;
        debugOffset.Y = 0;
        spriteBatch.Draw(Pixel, new Vector2(BoundLeft, BoundTop), null, color, 0, debugOffset, debugScale, SpriteEffects.None, 0);
    }
}