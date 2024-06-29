using System.Collections.Generic;
using UnityEngine;
using Path = System.Collections.Generic.List<Movement>;

public class HybridAStarScene : MonoBehaviour
{
    [SerializeField] private MeshRenderer _floorMeshRenderer;

    [SerializeField] private LayerMask _unwalkableMask;

    [SerializeField] private LineRenderer lineRenderer;

    [SerializeField] private float _cellSize = 0.5f;

    private Grid<Cell> _grid;

    [SerializeField] private Car _startCar, _goalCar;

    private Grid<AStarCell> _holonomicCostGrid;
    private Grid<FlowFieldCell> _flowFieldCostGrid;
    private Grid<VoronoiFieldCell> _voronoiFieldCostGrid;

    private float[,] _euclideanCosts;
    private float[,] _holonomicCosts;
    private float[,] _flowFieldCosts;
    private float[,] _voronoiFieldCosts;

    private Path _path;

    private List<Node> _expandedNodes;

    void OnDrawGizmos()
    {
        //DisplayGrid();

        DisplayPath();

        DisplayExpandedNodes();

        if (_floorMeshRenderer == null)
        {
            _floorMeshRenderer = GetComponent<MeshRenderer>();
        }

        if (_startCar == null) return;

        if (_goalCar == null) return;

        if (_holonomicCostGrid != null) return;
        if (_flowFieldCosts != null) return;
        if (_voronoiFieldCosts != null) return;

        CalculateCosts();
    }

    public void RecalculateCosts()
    {
        CalculateCosts();
    }

    public void FindPath()
    {
        _path = HybridAStar.FindPath(_grid, _startCar, _goalCar, _euclideanCosts, _holonomicCosts, _flowFieldCosts,
            _voronoiFieldCosts, out var _, out _expandedNodes);
    }

    private void CalculateCosts()
    {
        CreateGrid();

        GenerateHolonomicCostGrid();

        GenerateFlowFieldCostGrid();

        GenerateVoronoiFieldCostGrid();

        _euclideanCosts = new float[_grid.Width, _grid.Height];
        _holonomicCosts = new float[_grid.Width, _grid.Height];
        _flowFieldCosts = new float[_grid.Width, _grid.Height];
        _voronoiFieldCosts = new float[_grid.Width, _grid.Height];

        for (var x = 0; x < _grid.Width; x++)
        {
            for (var y = 0; y < _grid.Height; y++)
            {
                var currentCell = _grid.Cells[x, y];

                var distance = (currentCell.Center - _goalCar.RearWheelPosition).magnitude;

                _euclideanCosts[x, y] = distance;
                _holonomicCosts[x, y] = _holonomicCostGrid.Cells[x, y].fCost;
                _flowFieldCosts[x, y] = _flowFieldCostGrid.Cells[x, y].Cost;
                _voronoiFieldCosts[x, y] = _voronoiFieldCostGrid.Cells[x, y].FieldValue;
            }
        }
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

    private void GenerateHolonomicCostGrid()
    {
        if (_grid == null) return;

        if (_startCar == null) return;

        if (_goalCar == null) return;

        // TODO: Refactor this section to eliminate redundant obstacle checks.
        _holonomicCostGrid = new Grid<AStarCell>(_grid.Size, _grid.Center, _grid.CellSize, _grid.UnwalkableMask,
            (center, isWalkable, x, y) => new AStarCell(center, isWalkable, x, y));

        if (!_holonomicCostGrid.TryGetCellFromWorldPosition(_startCar.RearWheelPosition, out var startCell)) return;

        if (!_holonomicCostGrid.TryGetCellFromWorldPosition(_goalCar.RearWheelPosition, out var goalCell)) return;

        // yes, it's reversed
        AStar.Run(_holonomicCostGrid, goalCell, startCell);
    }

    private void GenerateFlowFieldCostGrid()
    {
        if (_grid == null) return;

        if (_goalCar == null) return;

        // TODO: Refactor this section to eliminate redundant obstacle checks.
        _flowFieldCostGrid = new Grid<FlowFieldCell>(_grid.Size, _grid.Center, _grid.CellSize, _grid.UnwalkableMask,
            (center, isWalkable, x, y) => new FlowFieldCell(center, isWalkable, x, y));

        if (!_flowFieldCostGrid.TryGetCellFromWorldPosition(_goalCar.RearWheelPosition, out var goalCell)) return;

        var startCells = new List<FlowFieldCell>
        {
            goalCell
        };

        FlowField.CalculateFlowField(_flowFieldCostGrid, startCells);
    }

    private void GenerateVoronoiFieldCostGrid()
    {
        if (_grid == null) return;

        // TODO: Refactor this section to eliminate redundant obstacle checks.
        _voronoiFieldCostGrid = new Grid<VoronoiFieldCell>(_grid.Size, _grid.Center, _grid.CellSize,
            _grid.UnwalkableMask,
            (center, isWalkable, x, y) => new VoronoiFieldCell(center, isWalkable, x, y));

        VoronoiField.GenerateVoronoiField(_voronoiFieldCostGrid);
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

    private void DisplayPath()
    {
        if (_path == null) return;

        var waypoints = PathBuilder.GenerateWaypoints(_startCar.RearWheelPosition, _startCar.HeadingAngle, _path, 10);

        lineRenderer.positionCount = waypoints.Count;
        lineRenderer.SetPositions(waypoints.ToArray());
    }

    public void DisplayExpandedNodes()
    {
        if (_expandedNodes == null)
        {
            return;
        }

        foreach (var node in _expandedNodes)
        {
            if (node.Parent == null)
            {
                continue;
            }

            Gizmos.color = node.MovementInfo.GearVal == Movement.Gear.Backward ? Color.cyan : Color.green;

            Gizmos.DrawLine(node.RearWheelPosition, node.Parent.RearWheelPosition);
        }
    }
}