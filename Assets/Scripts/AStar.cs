using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar
{
    AStarCell[,] cells;

    public AStar(AStarCell[,] _cells)
    {
        cells = _cells;
    }

    public List<AStarCell> Run(AStarCell startCell, AStarCell goalCell)
    {
        Heap<AStarCell> openSet = new Heap<AStarCell>(10000000);
        HashSet<AStarCell> closedSet = new HashSet<AStarCell>();


        openSet.Add(startCell);

        while (openSet.Count > 0)
        {
            AStarCell currentCell = openSet.RemoveFirst();
            closedSet.Add(currentCell);

            foreach (AStarCell neighbour in GetNeighbours(currentCell))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }

                float gCost = currentCell.gCost + GetDistance(neighbour, currentCell);

                if (gCost < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = gCost;
                    neighbour.hCost = GetDistance(goalCell, currentCell);
                    neighbour.parent = currentCell;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                    else
                    {
                        // openSet.UpdateItem(neighbour);
                    }
                }
            }
        }

        return RetracePath(startCell, goalCell);
    }

    List<AStarCell> RetracePath(AStarCell startCell, AStarCell goalCell)
    {
        List<AStarCell> path = new List<AStarCell>();
        AStarCell currentCell = goalCell;

        while (currentCell != startCell)
        {
            path.Add(currentCell);
            currentCell = currentCell.parent;
        }

        path.Reverse();

        return path;
    }

    public float GetDistance(AStarCell cellA, AStarCell cellB)
    {
        return (cellA.position - cellB.position).magnitude;
    }

    public List<AStarCell> GetNeighbours(AStarCell cell)
    {
        List<AStarCell> neighbours = new List<AStarCell>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }

                int checkX = cell.gridPosition.x + x;
                int checkY = cell.gridPosition.y + y;

                if (checkX >= 0 && checkX < cells.GetLength(0) && checkY >= 0 && checkY < cells.GetLength(1))
                {
                    neighbours.Add(cells[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }
}