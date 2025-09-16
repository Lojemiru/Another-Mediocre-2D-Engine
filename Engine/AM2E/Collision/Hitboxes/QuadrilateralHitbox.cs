
namespace AM2E.Collision;

public sealed class QuadrilateralHitbox : PolygonHitbox
{
    public QuadrilateralHitbox(int width, int height, int originX = 0, int originY = 0, int offsetX = 0, int offsetY = 0) 
        : base(4, originX, originY)
    {
        SetPoint(0, -offsetX, -offsetY);
        SetPoint(1, width - offsetX, -offsetY);
        SetPoint(2, width - offsetX, height - offsetY);
        SetPoint(3, -offsetX, height - offsetY);
        RecalculateBounds();
    }
}