namespace AM2E.Collision;

internal sealed class CollisionGridLooseCellNode
{
    internal CollisionGridLooseCellNode Next;

    internal int Index;

    internal CollisionGridLooseCellNode(int index)
    {
        Index = index;
    }
}