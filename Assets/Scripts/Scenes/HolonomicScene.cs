using System.Collections.Generic;
using UnityEngine;

public class HolonomicScene : MonoBehaviour
{
    private MeshRenderer _floorMeshRenderer;

    [SerializeField] private LayerMask _unwalkableMask;

    [SerializeField] private float _cellSize = 0.5f;

    [SerializeField] private Transform _startTransform, _goalTransform;

    private Grid<AStarCell> _grid;

    private List<AStarCell> _path;

    void OnDrawGizmos()
    {
        if (_floorMeshRenderer == null)
        {
            _floorMeshRenderer = GetComponent<MeshRenderer>();
        }

        CreateGrid();

        RunAStar();

        DisplayGrid();
    }

    private void CreateGrid()
    {
        var bounds = _floorMeshRenderer.bounds;
        var floorCenter = bounds.center;
        var size = bounds.size;

        if (_cellSize <= 0) return;

        _grid = new Grid<AStarCell>(size, floorCenter, _cellSize, _unwalkableMask,
            (center, isWalkable, x, y) => new AStarCell(center, isWalkable, x, y));
    }

    private void RunAStar()
    {
        if (_grid == null) return;

        if (_startTransform == null || _goalTransform == null) return;

        var startPosition = _startTransform.position;
        var goalPosition = _goalTransform.position;

        if (!_grid.TryGetCellFromWorldPosition(startPosition, out var startCell)) return;

        if (!_grid.TryGetCellFromWorldPosition(goalPosition, out var goalCell)) return;


        _path = AStar.Run(_grid, startCell, goalCell);
    }

    private void DisplayGrid()
    {
        if (_grid == null) return;

        if (_cellSize <= 0) return;

        var maxCost = AStar.GetMaxCost(_grid);

        for (var i = 0; i < _grid.Width; i++)
        {
            for (var j = 0; j < _grid.Height; j++)
            {
                var cell = _grid.Cells[i, j];

                Gizmos.color = GetCellColor(cell, maxCost);

                var size = new Vector3(_cellSize - 0.1f, 0.1f, _cellSize - 0.1f);

                Gizmos.DrawCube(cell.Center, size);
            }
        }
    }

    private Color GetCellColor(AStarCell cell, float maxCost)
    {
        var cost = cell.fCost;

        if (!cell.IsWalkable)
        {
            return Color.black;
        }

        if (_path != null && _path.Contains(cell))
        {
            return Color.gray;
        }

        var grayValue = cost / maxCost;

        return new Color(grayValue, 1 - grayValue, 0);
    }
}