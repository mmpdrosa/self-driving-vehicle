using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AStartGrid : MonoBehaviour
{
    public AStarCell[,] cells;
    private float cellSize = 0.5f;
    public int width, height;

    [SerializeField] private LayerMask unwalkableMask;
    [SerializeField] private MeshRenderer floorMeshRenderer;
    [SerializeField] private Car startCar, endCar;

    private AStarCell startCell;
    private AStarCell goalCell;
    private AStar aStar;

    private List<AStarCell> aStarPath;

    private void Awake()
    {
        Vector3 size = floorMeshRenderer.bounds.size;
        width = Mathf.RoundToInt(size.x / cellSize);
        height = Mathf.RoundToInt(size.z / cellSize);

        CreateGrid();

        aStar = new AStar(cells);
    }

    private void OnDrawGizmos()
    {
        // DisplayGrid();
    }

    public void CreateGrid()
    {
        cells = new AStarCell[width, height];

        Vector3 center = floorMeshRenderer.bounds.center;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector3 position = new Vector3(
                    i * cellSize - (width / 2 * cellSize) + center.x + cellSize / 2,
                    0,
                    j * cellSize - (height / 2 * cellSize) + center.z + cellSize / 2
                );

                bool walkable = !Physics.CheckSphere(position, cellSize, unwalkableMask);

                cells[i, j] = new AStarCell(position, walkable, new Vector2Int(i, j));
            }
        }
    }

    private void ResetCells()
    {
        foreach (AStarCell cell in cells)
        {
            cell.gCost = 0;
            cell.hCost = 0;
            cell.parent = null;
        }
    }

    public void Run()
    {
        startCell = GetCellFromWorldPosition(startCar.rearWheelPosition);
        goalCell = GetCellFromWorldPosition(endCar.rearWheelPosition);

        if (IsPositionWalkable(startCar.rearWheelPosition) && IsPositionWalkable(endCar.rearWheelPosition))
        {
            ResetCells();
            aStarPath = aStar.Run(startCell, goalCell);

            foreach (AStarCell cell in cells)
            {
                cell.gCost += (cell.position - goalCell.position).magnitude;
                // cell.hCost = 0;
            }
        }
    }

    public AStarCell GetCellFromWorldPosition(Vector3 worldPosition)
    {
        Vector3 center = floorMeshRenderer.bounds.center;
        Vector3 relativePosition = worldPosition - center;

        int i = Mathf.FloorToInt((relativePosition.x + width / 2 * cellSize) / cellSize);
        int j = Mathf.FloorToInt((relativePosition.z + height / 2 * cellSize) / cellSize);

        if (i < 0 || i >= width || j < 0 || j >= height)
        {
            return null;
        }

        return cells[i, j];
    }

    public bool IsPositionWalkable(Vector3 worldPosition)
    {
        AStarCell cell = GetCellFromWorldPosition(worldPosition);
        if (cell != null)
        {
            return cell.walkable;
        }

        return false;
    }

    public void DisplayGrid()
    {
        if (cells == null)
        {
            return;
        }

        float maxHeuristics = 0;

        foreach (AStarCell cell in cells)
        {
            if (cell.fCost > maxHeuristics)
            {
                maxHeuristics = cell.fCost;
            }
        }

        foreach (AStarCell cell in cells)
        {
            float t = Mathf.InverseLerp(0, maxHeuristics, cell.fCost);
            Color lerpedColor = Color.Lerp(Color.green, Color.red, t);

            Gizmos.color = cell.walkable ? lerpedColor : Color.red;

            if (aStarPath.Contains(cell))
            {
                Gizmos.color = Color.black;
            }

            if (cell == startCell || cell == goalCell)
            {
                Gizmos.color = Color.blue;
            }

            Gizmos.DrawCube(cell.position, new Vector3(cellSize * 0.9f, 0, cellSize * 0.9f));
        }
    }
}