using System;
using AM2E.Levels;

namespace AM2E.Collision;

internal sealed class CollisionGridLooseCell
{
    private CollisionGrid grid;
    internal ColliderBase Head;
    private int index;

    internal int Left = int.MaxValue, 
                 Right = int.MinValue, 
                 Top = int.MinValue, 
                 Bottom = int.MaxValue;

    internal CollisionGridLooseCell(CollisionGrid grid, int index)
    {
        this.grid = grid;
        this.index = index;
    }
    
    internal void Insert(ColliderBase collider)
    {
        collider.Head = Head;
        Head = collider;

        UpdateBounds(collider);
    }

    internal void Remove(ColliderBase collider)
    {
        ColliderBase current = null;
        var next = Head;
        while (next is not null)
        {
            if (next == collider)
            {
                if (current == null)
                {
                    Head = next.Head;
                }
                else
                {
                    current.Head = next.Head;
                }

                return;
            }

            current = next;
            next = current.Head;
        }
    } 

    private void UpdateBounds(ColliderBase collider)
    {
        var oldLeftMost = Math.Clamp(Left / grid.CellsWide, 0, grid.CellsWide - 1);
        var oldRightMost = Math.Clamp(Right / grid.CellsWide, 0, grid.CellsWide - 1);
        var oldTopMost = Math.Clamp(Top / grid.CellsHigh, 0, grid.CellsHigh - 1);
        var oldBottomMost = Math.Clamp(Bottom / grid.CellsHigh, 0, grid.CellsHigh - 1);

        // Expand the cell bounds.
        Left = Math.Min(Left, collider.BoundLeft);
        Right = Math.Max(Right, collider.BoundRight);
        Top = Math.Min(Top, collider.BoundTop);
        Bottom = Math.Max(Bottom, collider.BoundBottom);

        var leftMost = Math.Clamp(Left / grid.CellsWide, 0, grid.CellsWide - 1);
        var rightMost = Math.Clamp(Right / grid.CellsWide, 0, grid.CellsWide - 1);
        var topMost = Math.Clamp(Top / grid.CellsHigh, 0, grid.CellsHigh - 1);
        var bottomMost = Math.Clamp(Bottom / grid.CellsHigh, 0, grid.CellsHigh - 1);

        if (oldLeftMost != leftMost || oldRightMost != rightMost || oldTopMost != topMost ||
            oldBottomMost != bottomMost)
        {
            // Remove self from all fixed grid nodes.
            for (var i = oldLeftMost; i <= oldRightMost; i++)
            {
                for (var j = oldTopMost; j <= oldBottomMost; j++)
                {
                    grid.TightCells[(j * grid.CellsHigh) + i].Remove(index);
                }
            }

            // Insert self into fixed grid nodes.
            for (var i = leftMost; i <= rightMost; i++)
            {
                for (var j = topMost; j <= bottomMost; j++)
                {
                    grid.TightCells[(j * grid.CellsHigh) + i].Insert(new CollisionGridLooseCellNode(index));
                }
            }
        }
    }
}