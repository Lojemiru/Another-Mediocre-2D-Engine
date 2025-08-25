
namespace AM2E.Collision;

public sealed class QuadrilateralHitbox : PolygonHitbox
{
    public QuadrilateralHitbox(int width, int height, int originX = 0, int originY = 0) 
        : base(4, originX, originY)
    {
        SetPoint(0, 0, 0);
        SetPoint(1, width, 0);
        SetPoint(2, width, height);
        SetPoint(3, 0, height);
        RecalculateBounds();
    }
}