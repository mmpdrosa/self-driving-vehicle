using System;
using UnityEngine;

public class Grid<T> where T : Cell
{
    public Vector3 Size { get; }

    public Vector3 Center { get; }

    public float CellSize { get; }

    public LayerMask UnwalkableMask;

    private Func<Vector3, bool, int, int, T> _cellFactory;

    public T[,] Cells;

    public int Width { get; }

    public int Height { get; }

    public int MaxSize => Width * Height;

    public Grid(Vector3 size, Vector3 center, float cellSize, LayerMask unwalkableMask,
        Func<Vector3, bool, int, int, T> cellFactory)
    {
        Size = size;
        Center = center;
        CellSize = cellSize;
        UnwalkableMask = unwalkableMask;
        _cellFactory = cellFactory;

        Width = Mathf.RoundToInt(size.x / cellSize);
        Height = Mathf.RoundToInt(size.z / cellSize);

        CreateGrid();
    }

    public void CreateGrid()
    {
        Cells = new T[Width, Height];

        for (var i = 0; i < Width; i++)
        {
            for (var j = 0; j < Height; j++)
            {
                var position = new Vector3(
                    i * CellSize - (Width - 1) / 2f * CellSize + Center.x,
                    Center.y,
                    j * CellSize - (Height - 1) / 2f * CellSize + Center.z
                );

                var isWalkable = !CheckCollision(position);

                Cells[i, j] = _cellFactory(position, isWalkable, i, j);
            }
        }
    }

    public bool CheckCollision(Vector3 position)
    {
        var collided =
            Physics.CheckBox(position, new Vector3(CellSize / 2f, 0.1f, CellSize / 2f), Quaternion.identity,
                UnwalkableMask);

        return collided;
    }

    public bool TryGetCell(int x, int y, out T cell)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            cell = null;
            return false;
        }

        cell = Cells[x, y];
        return true;
    }

    public bool TryGetCellFromWorldPosition(Vector3 position, out T cell)
    {
        var relativePosition = position - Center;

        var x = Mathf.FloorToInt((relativePosition.x + Width / 2f * CellSize) / CellSize);
        var y = Mathf.FloorToInt((relativePosition.z + Height / 2f * CellSize) / CellSize);

        return TryGetCell(x, y, out cell);
    }

    public bool IsPositionWalkable(Vector3 position)
    {
        if (!TryGetCellFromWorldPosition(position, out var cell))
        {
            return false;
        }

        return cell.IsWalkable;
    }
}