using UnityEngine;

public class GridScene : MonoBehaviour
{
    [SerializeField] private MeshRenderer _floorMeshRenderer;

    [SerializeField] private LayerMask _unwalkableMask;

    [SerializeField] private float _cellSize = 0.5f;

    private Grid<Cell> _grid;

    void OnDrawGizmos()
    {
        if (_floorMeshRenderer == null)
        {
            _floorMeshRenderer = GetComponent<MeshRenderer>();
        }

        CreateGrid();

        DisplayGrid();
    }

    private void CreateGrid()
    {
        var bounds = _floorMeshRenderer.bounds;
        var floorCenter = bounds.center;
        var size = bounds.size;

        if (_cellSize <= 0) return;

        _grid = new Grid<Cell>(size, floorCenter, _cellSize, _unwalkableMask,
            (center, isWalkable, x, y) => new Cell(center, isWalkable, x, y));
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

    private Color GetCellColor(Cell cell)
    {
        return cell.IsWalkable ? Color.white : Color.black;
    }
}