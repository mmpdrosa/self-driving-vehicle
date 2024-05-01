using UnityEngine;
using Path = System.Collections.Generic.List<Movement>;

public class Controller : MonoBehaviour
{
    [SerializeField] private LayerMask _unwalkableMask;

    [SerializeField] private MeshRenderer _floorMeshRenderer;

    [SerializeField] private LineRenderer lineRenderer;

    [SerializeField] private Car _startCar, _goalCar;

    private AStartGrid _grid;

    private HybridAStar _hybridAStar;

    public void Start()
    {
        InitGrid();
        EuclideanHeuristic();
        HolonomicHeuristic();
        InitHybridAStar();
    }

    public void Update()
    {
    }

    public void OnDrawGizmos()
    {
        if (_startCar != null)
        {
            _startCar.DrawShape();
        }

        if (_goalCar != null)
        {
            _goalCar.DrawShape();
        }

        //_grid?.DrawGrid();

        _hybridAStar?.DrawExpandedNodes();
        _hybridAStar?.DrawFinalNodes();
    }

    private void InitGrid()
    {
        var floorSize = _floorMeshRenderer.bounds.size;

        var width = Mathf.RoundToInt(floorSize.x / Constants.CellSize);
        var height = Mathf.RoundToInt(floorSize.z / Constants.CellSize);

        var floorCenter = _floorMeshRenderer.bounds.center;

        _grid = new AStartGrid(width, height, floorCenter, _unwalkableMask);
    }

    private void EuclideanHeuristic()
    {
        for (var i = 0; i < _grid.Width; i++)
        {
            for (var j = 0; j < _grid.Height; j++)
            {
                var cell = _grid.Cells[i, j];

                cell.EuclideanDistance = Vector3.Distance(cell.Position, _goalCar.RearWheelPosition);
            }
        }
    }

    private void HolonomicHeuristic()
    {
        AStar aStar = new(_grid);

        var startCell = _grid.GetCell(_goalCar.RearWheelPosition);

        var goalCell = _grid.GetCell(_startCar.RearWheelPosition);

        aStar.Run(startCell, goalCell);
    }

    private void NonholonomicHeuristic()
    {
    }

    private void InitHybridAStar()
    {
        _hybridAStar = new HybridAStar(_grid, _startCar, _goalCar);

        var path = _hybridAStar.FindPath(analyticalExpansion: true);

        if (path != null)
        {
            DisplayFinalPath(path);
        }
    }

    private void PathSmoothing()
    {
    }

    private void DisplayFinalPath(Path path)
    {
        var waypoints = PathBuilder.GenerateWaypoints(_startCar.RearWheelPosition, _startCar.HeadingAngle, path, 10);

        lineRenderer.positionCount = waypoints.Count;
        lineRenderer.SetPositions(waypoints.ToArray());
    }
}