using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class VoronoiField
{
    public static void GenerateVoronoiField(Grid<VoronoiFieldCell> grid)
    {
        FindObstacleRegions(grid);

        FindVoronoiRegions(grid);

        FindVoronoiEdges(grid);

        FindDistancesToEdges(grid);

        for (var i = 0; i < grid.Width; i++)
        {
            for (var j = 0; j < grid.Height; j++)
            {
                var currentCell = grid.Cells[i, j];

                float rho;

                if (!currentCell.IsWalkable)
                {
                    rho = 1f;
                }
                else if (currentCell.IsEdge)
                {
                    rho = 0;
                }
                else
                {
                    var closestObstacleDistance = currentCell.ClosestObstacleDistance;
                    var closestEdgeDistance = currentCell.ClosestEdgeDistance;
                    var alpha = Constants.VoronoiAlpha;
                    var maxObstacleDistance = Constants.VoronoiMaxObstacleDistance;

                    rho = (alpha / (alpha + closestObstacleDistance)) *
                          (closestEdgeDistance / (closestObstacleDistance + closestEdgeDistance)) *
                          (((closestObstacleDistance - maxObstacleDistance) *
                            (closestObstacleDistance - maxObstacleDistance)) /
                           (maxObstacleDistance * maxObstacleDistance));
                }

                currentCell.SetFieldValue(rho);
            }
        }
    }

    private static void FindObstacleRegions(Grid<VoronoiFieldCell> grid)
    {
        var region = 0;

        for (var x = 0; x < grid.Width; x++)
        {
            for (var y = 0; y < grid.Height; y++)
            {
                var cell = grid.Cells[x, y];

                if (cell.IsWalkable) continue;

                if (cell.Region != -1) continue;

                FloodFillObstacle(grid, cell, region);

                region++;
            }
        }
    }

    private static void FloodFillObstacle(Grid<VoronoiFieldCell> grid, VoronoiFieldCell startCell, int region)
    {
        var openSet = new Queue<VoronoiFieldCell>();

        openSet.Enqueue(startCell);

        while (openSet.Count > 0)
        {
            var currentCell = openSet.Dequeue();

            currentCell.SetRegion(region);

            foreach (var direction in Utils.DirectionsWithoutCorners)
            {
                var x = currentCell.GridPosition.x + direction.x;
                var y = currentCell.GridPosition.y + direction.y;

                if (!grid.TryGetCell(x, y, out var neighbor)) continue;

                if (neighbor.IsWalkable) continue;

                if (neighbor.Region != -1) continue;

                if (openSet.Contains(neighbor)) continue;

                openSet.Enqueue(neighbor);
            }
        }
    }

    private static void FindVoronoiRegions(Grid<VoronoiFieldCell> grid)
    {
        // TODO: Refactor this section to eliminate redundant obstacle checks.
        var flowFieldGrid = new Grid<FlowFieldCell>(grid.Size, grid.Center, grid.CellSize, grid.UnwalkableMask,
            (center, _, x, y) => new FlowFieldCell(center, true, x, y));

        for (var x = 0; x < grid.Width; x++)
        {
            for (var y = 0; y < grid.Height; y++)
            {
                var flowFieldCell = flowFieldGrid.Cells[x, y];

                var voronoiFieldCell = grid.Cells[x, y];

                flowFieldCell.SetRegion(voronoiFieldCell.Region);
            }
        }

        var startCells = new List<FlowFieldCell>();

        for (var x = 0; x < grid.Width; x++)
        {
            for (var y = 0; y < grid.Height; y++)
            {
                var voronoiFieldCell = grid.Cells[x, y];

                if (voronoiFieldCell.IsWalkable) continue;

                var flowFieldCell = flowFieldGrid.Cells[x, y];

                startCells.Add(flowFieldCell);
            }
        }

        FlowField.CalculateFlowField(flowFieldGrid, startCells);

        for (var x = 0; x < grid.Width; x++)
        {
            for (var y = 0; y < grid.Height; y++)
            {
                var voronoiFieldCell = grid.Cells[x, y];

                var flowFieldCell = flowFieldGrid.Cells[x, y];

                var closestObstacleDistance = flowFieldCell.Cost;

                voronoiFieldCell.SetClosestObstacleDistance(closestObstacleDistance);
                voronoiFieldCell.SetRegion(flowFieldCell.Region);

                var closestObstacleCells = flowFieldCell.ClosestStartCells
                    .Select(startCell => grid.Cells[startCell.GridPosition.x, startCell.GridPosition.y])
                    .ToList();

                voronoiFieldCell.SetClosestObstacleCells(closestObstacleCells);

                if (closestObstacleCells.Count == 0) continue;

                foreach (var obstacleCell in closestObstacleCells)
                {
                    var distance = (voronoiFieldCell.Center - obstacleCell.Center).magnitude;

                    voronoiFieldCell.SetClosestObstacleDistance(distance);

                    break;
                }
            }
        }
    }

    private static void FindVoronoiEdges(Grid<VoronoiFieldCell> grid)
    {
        for (var i = 0; i < grid.Width; i++)
        {
            for (var j = 0; j < grid.Height; j++)
            {
                var currentCell = grid.Cells[i, j];

                var currentRegion = currentCell.Region;

                foreach (var direction in Utils.DirectionsWithoutCorners)
                {
                    var x = i + direction.x;
                    var y = j + direction.y;

                    if (!grid.TryGetCell(x, y, out var neighbor)) continue;

                    if (neighbor.Region == currentRegion) continue;

                    currentCell.SetIsEdge(true);

                    break;
                }
            }
        }
    }

    private static void FindDistancesToEdges(Grid<VoronoiFieldCell> grid)
    {
        // TODO: Refactor this section to eliminate redundant obstacle checks.
        var flowFieldGrid = new Grid<FlowFieldCell>(grid.Size, grid.Center, grid.CellSize, grid.UnwalkableMask,
            (center, _, x, y) => new FlowFieldCell(center, true, x, y));

        var startCells = new List<FlowFieldCell>();

        for (var x = 0; x < grid.Width; x++)
        {
            for (var y = 0; y < grid.Height; y++)
            {
                var voronoiFieldCell = grid.Cells[x, y];

                if (!voronoiFieldCell.IsEdge) continue;

                var flowFieldCell = flowFieldGrid.Cells[x, y];

                startCells.Add(flowFieldCell);
            }
        }

        FlowField.CalculateFlowField(flowFieldGrid, startCells);

        for (var x = 0; x < grid.Width; x++)
        {
            for (var y = 0; y < grid.Height; y++)
            {
                var voronoiFieldCell = grid.Cells[x, y];

                var flowFieldCell = flowFieldGrid.Cells[x, y];

                var closestEdgeDistance = flowFieldCell.Cost;

                voronoiFieldCell.SetClosestEdgeDistance(closestEdgeDistance);

                var closestEdgeCells = flowFieldCell.ClosestStartCells
                    .Select(startCell => grid.Cells[startCell.GridPosition.x, startCell.GridPosition.y])
                    .ToList();

                voronoiFieldCell.SetClosestEdgeCells(closestEdgeCells);

                if (closestEdgeCells.Count == 0) continue;

                foreach (var edgeCell in closestEdgeCells)
                {
                    var distance = (voronoiFieldCell.Center - edgeCell.Center).magnitude;

                    voronoiFieldCell.SetClosestEdgeDistance(distance);

                    break;
                }
            }
        }
    }
}