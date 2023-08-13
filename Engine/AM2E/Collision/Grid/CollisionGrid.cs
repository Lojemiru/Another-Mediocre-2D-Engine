using System;
using AM2E.Levels;

namespace AM2E.Collision;

internal sealed class CollisionGrid
{
    private CollisionGridLooseCell[] looseCells;
    internal CollisionGridTightCell[] TightCells;

    internal readonly int CellsWide;
    internal readonly int CellsHigh;
    internal readonly int CellWidth;
    internal readonly int CellHeight;
    
    internal CollisionGrid(Level level, int cellWidth, int cellHeight)
    {
        CellWidth = cellWidth;
        CellHeight = cellHeight;

        CellsWide = level.Width / cellWidth;
        CellsHigh = level.Height / cellHeight;

        looseCells = new CollisionGridLooseCell[CellsWide * CellsHigh];
        TightCells = new CollisionGridTightCell[CellsWide * CellsHigh];
    }

    internal void Insert(ColliderBase collider)
    {
        var cellX = Math.Clamp(collider.X / CellWidth, 0, CellsWide - 1);
        var cellY = Math.Clamp(collider.Y / CellHeight, 0, CellsHigh - 1);
        var cellID = (cellY * CellsHigh) + cellX;

        looseCells[cellID] ??= new CollisionGridLooseCell(this, cellID);

        looseCells[cellID].Insert(collider);
    }

    internal void Remove(ColliderBase collider)
    {
        var cellX = Math.Clamp(collider.X / CellWidth, 0, CellsWide - 1);
        var cellY = Math.Clamp(collider.Y / CellHeight, 0, CellsHigh - 1);
        var cellID = (cellY * CellsHigh) + cellX;

        looseCells[cellID].Remove(collider);
    }
}