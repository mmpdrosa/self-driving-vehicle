using System.Collections.Generic;

public class AStar
{
    public static List<AStarCell> Run(Grid<AStarCell> grid, AStarCell startCell, AStarCell goalCell)
    {
        for (var i = 0; i < grid.Width; i++)
        {
            for (var j = 0; j < grid.Height; j++)
            {
                var cell = grid.Cells[i, j];

                cell.Reset();
            }
        }

        Heap<AStarCell> openSet = new(grid.MaxSize);
        HashSet<AStarCell> closedSet = new();

        openSet.Add(startCell);

        while (openSet.Count > 0)
        {
            var currentCell = openSet.RemoveFirst();

            closedSet.Add(currentCell);

            foreach (var neighbor in GetNeighboringCells(grid, currentCell))
            {
                if (!neighbor.IsWalkable) continue;

                if (closedSet.Contains(neighbor)) continue;

                var distanceToNeighbor = (currentCell.Center - neighbor.Center).magnitude;

                var gCost = currentCell.gCost + distanceToNeighbor;

                if (openSet.Contains(neighbor) && gCost >= neighbor.gCost) continue;

                var distanceToGoal = (currentCell.Center - goalCell.Center).magnitude;

                neighbor.SetCosts(gCost, hCost: distanceToGoal);
                neighbor.SetParent(currentCell);

                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
            }
        }

        return RetracePath(goalCell);
    }

    private static List<AStarCell> GetNeighboringCells(Grid<AStarCell> grid, Cell cell)
    {
        var neighbors = new List<AStarCell>();

        var directions = Utils.DirectionsWithCorners;

        foreach (var direction in directions)
        {
            var x = cell.GridPosition.x + direction.x;
            var y = cell.GridPosition.y + direction.y;

            if (!grid.TryGetCell(x, y, out var neighbor)) continue;

            if (!neighbor.IsWalkable) continue;

            neighbors.Add(neighbor);
        }

        return neighbors;
    }

    private static List<AStarCell> RetracePath(AStarCell goalCell)
    {
        var path = new List<AStarCell>();

        var currentCell = goalCell;

        while (currentCell != null)
        {
            path.Add(currentCell);
            currentCell = currentCell.Parent;
        }

        path.Reverse();

        return path;
    }

    public static float GetMaxCost(Grid<AStarCell> grid)
    {
        var maxCost = float.MinValue;

        for (var i = 0; i < grid.Width; i++)
        {
            for (var j = 0; j < grid.Height; j++)
            {
                var cell = grid.Cells[i, j];

                var cost = cell.fCost;

                if (cost > maxCost)
                {
                    maxCost = cell.fCost;
                }
            }
        }

        return maxCost;
    }
}