using AM2E.Collision;

namespace AM2E.Levels;

// TODO: Actors currently do a lot of hitbox flip management that we proooobably would like to here instead...

public abstract class ColliderBase : GenericLevelElement, ICollider
{
    public Collider Collider { get; }
    
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

    protected ColliderBase(int x, int y, Layer layer) : base(x, y, layer)
    {
        Collider = new Collider(x, y);
    }
}