using System.Linq;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class AStartGrid
{
    public int Width { get; set; }
    public int Height { get; set; }

    private Vector3 _center;

    public LayerMask UnwalkableMask;

    public AStarCell[,] Cells;

    public int MaxSize => Width * Height;

    public AStartGrid(int width, int height, Vector3 center, LayerMask unwalkableMask)
    {
        Width = width;
        Height = height;

        _center = center;
        UnwalkableMask = unwalkableMask;

        Init();
    }

    private void Init()
    {
        Cells = new AStarCell[Width, Height];

        for (var i = 0; i < Width; i++)
        {
            for (var j = 0; j < Height; j++)
            {
                Vector3 position = new(
                    i * Constants.CellSize - Width / 2f * Constants.CellSize + _center.x + Constants.CellSize / 2,
                    _center.y,
                    j * Constants.CellSize - Height / 2f * Constants.CellSize + _center.z + Constants.CellSize / 2
                );

                var walkable = !CheckCollision(position);

                Cells[i, j] = new AStarCell(new Vector2Int(i, j), position, walkable);
            }
        }
    }

    public bool CheckCollision(Vector3 position)
    {
        var collided =
            Physics.CheckSphere(position, Mathf.Sqrt(Constants.CellSize * Constants.CellSize) * 2, UnwalkableMask);

        return collided;
    }

    public AStarCell GetCell(Vector3 position)
    {
        var relativePosition = position - _center;

        var i = Mathf.FloorToInt((relativePosition.x + Width / 2f * Constants.CellSize) / Constants.CellSize);
        var j = Mathf.FloorToInt((relativePosition.z + Height / 2f * Constants.CellSize) / Constants.CellSize);

        if (i < 0 || i >= Width || j < 0 || j >= Height)
        {
            return null;
        }

        return Cells[i, j];
    }

    public bool IsPositionWalkable(Vector3 position)
    {
        return GetCell(position)?.Walkable == true;
    }

    public static float GetDistance(AStarCell cellA, AStarCell cellB)
    {
        return (cellA.Position - cellB.Position).magnitude;
    }

    public void DrawGrid()
    {
        if (Cells == null)
        {
            return;
        }

        //var maxEuclideanDistance =
        //    Cells.Cast<AStarCell>().Aggregate(0f, (max, cell) => Mathf.Max(max, cell.EuclideanDistance));

        var maxHolonomicHeuristic =
            Cells.Cast<AStarCell>().Aggregate(0f, (max, cell) => Mathf.Max(max, cell.HolonomicHeuristic));

        foreach (var cell in Cells)
        {
            //var t = Mathf.InverseLerp(0, maxEuclideanDistance, cell.EuclideanDistance);

            var t = Mathf.InverseLerp(0, maxHolonomicHeuristic, cell.HolonomicHeuristic);

            var lerpedColor = Color.Lerp(Color.green, Color.red, t);

            Gizmos.color = cell.Walkable ? lerpedColor : Color.red;

            //Gizmos.color = cell.Walkable ? Color.white : Color.red;

            Vector3 size = new(Constants.CellSize, 0f, Constants.CellSize);

            Gizmos.DrawWireCube(cell.Position, size);
        }
    }
}