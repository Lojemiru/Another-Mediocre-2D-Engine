
namespace AM2E.Collision;

public sealed class QuadrilateralHitbox : PolygonHitbox
{
    public QuadrilateralHitbox(int x, int y, int width, int height, int offsetX = 0, int offsetY = 0) 
        : base(4, x, y, offsetX, offsetY)
    {
        SetPoint(0, 0, 0);
        SetPoint(1, width, 0);
        SetPoint(2, width, height);
        SetPoint(3, 0, height);
        RecalculateBounds();
    }
}