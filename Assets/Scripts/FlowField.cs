using System.Collections.Generic;

public class FlowField
{
    public static void CalculateFlowField(Grid<FlowFieldCell> grid, List<FlowFieldCell> startCells)
    {
        for (var i = 0; i < grid.Width; i++)
        {
            for (var j = 0; j < grid.Height; j++)
            {
                var cell = grid.Cells[i, j];

                cell.Reset();

                var neighbors = GetNeighboringCells(grid, cell);

                cell.SetNeighbors(neighbors);
            }
        }

        var openSet = new Queue<FlowFieldCell>();
        var closedSet = new HashSet<FlowFieldCell>();

        foreach (var cell in startCells)
        {
            openSet.Enqueue(cell);

            cell.SetCost(0);

            cell.AddClosestStartCell(cell);

            closedSet.Add(cell);
        }

        while (openSet.Count > 0)
        {
            var currentCell = openSet.Dequeue();

            foreach (var neighbor in currentCell.Neighbors)
            {
                var distance = (currentCell.Center - neighbor.Center).magnitude;

                var newCost = currentCell.Cost + distance;

                if (newCost > neighbor.Cost) continue;

                neighbor.SetCost(newCost);

                neighbor.SetRegion(currentCell.Region);

                if (!newCost.Equals(neighbor.Cost))
                {
                    neighbor.ClearClosestStartCells();
                }

                foreach (var cell in currentCell.ClosestStartCells)
                {
                    neighbor.AddClosestStartCell(cell);
                }

                if (closedSet.Contains(neighbor)) continue;

                openSet.Enqueue(neighbor);

                closedSet.Add(neighbor);
            }
        }
    }

    private static HashSet<FlowFieldCell> GetNeighboringCells(Grid<FlowFieldCell> grid, Cell cell,
        bool includeCorners = true)
    {
        var neighbors = new HashSet<FlowFieldCell>();

        var directions = includeCorners ? Utils.DirectionsWithCorners : Utils.DirectionsWithoutCorners;

        var hasAnyNeighborObstacle = false;

        foreach (var direction in directions)
        {
            var x = cell.GridPosition.x + direction.x;
            var y = cell.GridPosition.y + direction.y;

            if (!grid.TryGetCell(x, y, out var neighbor)) continue;

            if (!neighbor.IsWalkable)
            {
                hasAnyNeighborObstacle = true;
                continue;
            }

            neighbors.Add(neighbor);
        }

        if (!includeCorners || !hasAnyNeighborObstacle) return neighbors;

        RemoveInvalidCornerNeighbors(grid, cell, neighbors);

        return neighbors;
    }

    private static void RemoveInvalidCornerNeighbors(Grid<FlowFieldCell> grid, Cell cell,
        HashSet<FlowFieldCell> neighbors)
    {
        foreach (var direction in Utils.CornerDirections)
        {
            var x = cell.GridPosition.x + direction.x;
            var y = cell.GridPosition.y + direction.y;

            if (!grid.TryGetCell(x, y, out var cornerNeighbor)) continue;

            var x1 = cell.GridPosition.x + direction.x;
            var y1 = cell.GridPosition.y;

            var x2 = cell.GridPosition.x;
            var y2 = cell.GridPosition.y + direction.y;

            if (!grid.TryGetCell(x1, y1, out var neighbor1) || !grid.TryGetCell(x2, y2, out var neighbor2)) continue;

            if (!neighbor1.IsWalkable || !neighbor2.IsWalkable)
            {
                neighbors.Remove(cornerNeighbor);
            }
        }
    }

    public static float GetMaxCost(Grid<FlowFieldCell> grid)
    {
        var maxCost = float.MinValue;

        for (var i = 0; i < grid.Width; i++)
        {
            for (var j = 0; j < grid.Height; j++)
            {
                var cell = grid.Cells[i, j];

                var cost = cell.Cost;

                if (cost > maxCost && cost < float.MaxValue)
                {
                    maxCost = cell.Cost;
                }
            }
        }

        return maxCost;
    }
}