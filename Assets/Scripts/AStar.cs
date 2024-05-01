using System.Collections.Generic;

public class AStar
{
    private readonly AStartGrid _grid;

    private readonly AStarCell[,] _cells;

    public AStar(AStartGrid grid)
    {
        _grid = grid;
        _cells = grid.Cells;
    }

    public List<AStarCell> Run(AStarCell startCell, AStarCell goalCell)
    {
        if (!_grid.IsPositionWalkable(startCell.Position) ||
            !_grid.IsPositionWalkable(goalCell.Position))
        {
            return null;
        }

        Heap<AStarCell> openSet = new(_grid.MaxSize);
        HashSet<AStarCell> closedSet = new();

        openSet.Add(startCell);

        while (openSet.Count > 0)
        {
            var currentCell = openSet.RemoveFirst();

            closedSet.Add(currentCell);

            foreach (var neighbour in GetNeighbours(currentCell))
            {
                if (!neighbour.Walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }

                var gCost = currentCell.gCost + AStartGrid.GetDistance(neighbour, currentCell);

                if (gCost < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = gCost;
                    neighbour.hCost = AStartGrid.GetDistance(goalCell, currentCell);
                    neighbour.Parent = currentCell;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }

        return RetracePath(startCell, goalCell);
    }

    private static List<AStarCell> RetracePath(AStarCell startCell, AStarCell goalCell)
    {
        List<AStarCell> path = new();
        var currentCell = goalCell;

        while (currentCell != startCell)
        {
            path.Add(currentCell);
            currentCell = currentCell.Parent;
        }

        path.Reverse();

        return path;
    }

    private List<AStarCell> GetNeighbours(AStarCell cell)
    {
        List<AStarCell> neighbours = new();

        for (var i = -1; i <= 1; i++)
        {
            for (var j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                var checkX = cell.GridPosition.x + i;
                var checkY = cell.GridPosition.y + j;

                if (checkX >= 0 && checkX < _cells.GetLength(0) && checkY >= 0 && checkY < _cells.GetLength(1))
                {
                    neighbours.Add(_cells[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }
}