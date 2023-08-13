namespace AM2E.Collision;

internal sealed class CollisionGridTightCell
{
    internal CollisionGridLooseCellNode Next;
    
    public void Insert(CollisionGridLooseCellNode cellNode)
    {
        cellNode.Next = Next;
        Next = cellNode;
    }

    public void Remove(int index)
    {
        CollisionGridLooseCellNode current = null;
        var next = Next;
        while (next is not null)
        {
            if (next.Index == index)
            {
                if (current == null)
                {
                    Next = next.Next;
                }
                else
                {
                    current.Next = next.Next;
                }

                return;
            }

            current = next;
            next = current.Next;
        }
    }
}