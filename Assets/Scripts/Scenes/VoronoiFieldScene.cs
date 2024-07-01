using System.Collections.Generic;
using UnityEngine;

public class VoronoiScene : MonoBehaviour
{
    private MeshRenderer _floorMeshRenderer;

    [SerializeField] private LayerMask _unwalkableMask;

    [SerializeField] private float _cellSize = 0.5f;

    private Grid<VoronoiFieldCell> _grid;

    private Dictionary<int, Color> _regionColors = new();

    void OnDrawGizmos()
    {
        if (_floorMeshRenderer == null)
        {
            _floorMeshRenderer = GetComponent<MeshRenderer>();
        }

        CreateGrid();

        GenerateVoronoiField();

        DisplayGrid();
    }

    private void CreateGrid()
    {
        var bounds = _floorMeshRenderer.bounds;
        var floorCenter = bounds.center;
        var size = bounds.size;

        if (_cellSize <= 0) return;

        _grid = new Grid<VoronoiFieldCell>(size, floorCenter, _cellSize, _unwalkableMask,
            (center, isWalkable, x, y) => new VoronoiFieldCell(center, isWalkable, x, y));
    }

    private void GenerateVoronoiField()
    {
        if (_grid == null) return;

        VoronoiField.GenerateVoronoiField(_grid);
    }

    private void DisplayGrid()
    {
        if (_grid == null) return;

        if (_cellSize <= 0) return;

        for (var i = 0; i < _grid.Width; i++)
        {
            for (var j = 0; j < _grid.Height; j++)
            {
                var cell = _grid.Cells[i, j];

                Gizmos.color = GetCellColor(cell);

                var size = new Vector3(_cellSize - 0.1f, 0.1f, _cellSize - 0.1f);

                Gizmos.DrawCube(cell.Center, size);
            }
        }
    }

    //private Color GetCellColor(VoronoiFieldCell cell)
    //{
    //    if (cell.Region == -1)
    //    {
    //        return Color.white;
    //    }

    //    if (cell.IsEdge)
    //    {
    //        return Color.black;
    //    }

    //    if (!_regionColors.ContainsKey(cell.Region))
    //    {
    //        _regionColors[cell.Region] = GetRandomColor();
    //    }

    //    return _regionColors[cell.Region];
    //}

    private Color GetCellColor(VoronoiFieldCell cell)
    {
        return Color.Lerp(Color.white, Color.black, cell.FieldValue);
    }


    private Color GetRandomColor()
    {
        return Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
    }
}