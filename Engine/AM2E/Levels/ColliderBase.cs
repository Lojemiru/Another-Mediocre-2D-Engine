using AM2E.Collision;
using AM2E.Graphics;

namespace AM2E.Levels;

public abstract class ColliderBase : GenericLevelElement, ICollider
{
    public Collider Collider { get; protected init; }
    
    public new int X
    {
        get => Collider.X;
        set => Collider.X = value;
    }

    public new int Y
    {
        get => Collider.Y;
        set => Collider.Y = value;
    }

    protected ColliderBase(int x, int y, Layer layer) : base(layer)
    {
        Collider = new Collider(x, y);
    }
}