
namespace AM2E.Collision;

public sealed class TriangleHitbox : PolygonHitbox
{
    public TriangleHitbox(int x1, int y1, int x2, int y2, int x3, int y3, int originX = 0, int originY = 0) 
        : base(3, originX, originY)
    {
        SetPoint(0, x1, y1);
        SetPoint(1, x2, y2);
        SetPoint(2, x3, y3);
        RecalculateBounds();
    }
}