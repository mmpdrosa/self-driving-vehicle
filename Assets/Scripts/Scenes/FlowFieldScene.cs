using System.Collections.Generic;
using UnityEngine;

public class FlowFieldScene : MonoBehaviour
{
    private MeshRenderer _floorMeshRenderer;

    [SerializeField] private LayerMask _unwalkableMask;

    [SerializeField] private float _cellSize = 0.5f;

    [SerializeField] private Transform _target;

    private Grid<FlowFieldCell> _grid;

    void OnDrawGizmos()
    {
        if (_floorMeshRenderer == null)
        {
            _floorMeshRenderer = GetComponent<MeshRenderer>();
        }

        CreateGrid();

        GenerateFlowField();

        DisplayGrid();
    }

    private void CreateGrid()
    {
        var bounds = _floorMeshRenderer.bounds;
        var floorCenter = bounds.center;
        var size = bounds.size;

        if (_cellSize <= 0) return;

        _grid = new Grid<FlowFieldCell>(size, floorCenter, _cellSize, _unwalkableMask,
            (center, isWalkable, x, y) => new FlowFieldCell(center, isWalkable, x, y));
    }

    private void GenerateFlowField()
    {
        if (_grid == null) return;

        if (_target == null) return;

        if (!_grid.TryGetCellFromWorldPosition(_target.position, out var targetCell)) return;

        var startCells = new List<FlowFieldCell>
        {
            targetCell
        };

        FlowField.CalculateFlowField(_grid, startCells);
    }

    private void DisplayGrid()
    {
        if (_grid == null) return;

        if (_cellSize <= 0) return;

        var maxCost = FlowField.GetMaxCost(_grid);

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

    private Color GetCellColor(FlowFieldCell cell, float maxCost)
    {
        var cost = cell.Cost;

        if (!cell.IsWalkable)
        {
            return Color.black;
        }

        if (cost.Equals(float.MaxValue))
        {
            return Color.blue;
        }

        var grayValue = cost / maxCost;

        return new Color(grayValue, 1 - grayValue, 0);
    }
}