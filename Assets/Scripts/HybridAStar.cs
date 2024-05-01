using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Path = System.Collections.Generic.List<Movement>;

public class HybridAStar
{
    private readonly AStartGrid _grid;

    private readonly Car _startCar, _goalCar;

    private readonly List<Node> _expandedNodes = new();

    private readonly List<Node> _finalNodes = new();

    public HybridAStar(AStartGrid grid, Car startCar, Car goalCar)
    {
        _grid = grid;
        _startCar = startCar;
        _goalCar = goalCar;
    }

    public Path FindPath(bool analyticalExpansion = true)
    {
        if (!_grid.IsPositionWalkable(_startCar.RearWheelPosition) ||
            !_grid.IsPositionWalkable(_goalCar.RearWheelPosition))
        {
            return null;
        }

        Heap<Node> openSet = new(Constants.Iterations * 6);

        var lowestCostSetList = new Dictionary<int, Node>[_grid.Width, _grid.Height];

        for (var i = 0; i < _grid.Width; i++)
        {
            for (var j = 0; j < _grid.Height; j++)
            {
                lowestCostSetList[i, j] = new Dictionary<int, Node>();
            }
        }

        Movement startMovement = new(0, Movement.Steering.Straight, Movement.Gear.Forward);

        Node startNode = new(_startCar.RearWheelPosition, _startCar.HeadingAngle, startMovement, null);

        openSet.Add(startNode);

        Path reedsSheppPath = null;

        Node finalNode = null;

        var iterations = 0;

        while (iterations < Constants.Iterations)
        {
            iterations++;

            var currentNode = openSet.RemoveFirst();

            if (analyticalExpansion && TryAnalyticalExpansion(currentNode.RearWheelPosition, currentNode.HeadingAngle,
                    out reedsSheppPath))
            {
                finalNode = currentNode;
                break;
            }

            var distance = (currentNode.RearWheelPosition - _goalCar.RearWheelPosition).sqrMagnitude;

            var goalPositionReached = distance < Constants.GoalDistanceThreshold;

            var goalAngleReached = Math.Abs(currentNode.HeadingAngle - _goalCar.HeadingAngle) <
                                   Constants.GoalHeadingAngleThreshold;

            var goalReached = goalPositionReached && goalAngleReached;

            if (goalReached)
            {
                finalNode = currentNode;

                break;
            }

            var children = GetNodeChildren(currentNode);

            _expandedNodes.Add(currentNode);

            foreach (var child in children)
            {
                var childCell = _grid.GetCell(child.RearWheelPosition);

                child.hCost = childCell.EuclideanDistance + childCell.HolonomicHeuristic;

                var roundedChildHeadingAngle =
                    Utils.RoundValueToNearestStep(child.HeadingAngle, Constants.HeadingAngleStep);

                var lowestCostSet = lowestCostSetList[childCell.GridPosition.x, childCell.GridPosition.y];

                if (lowestCostSet.TryGetValue(roundedChildHeadingAngle, out var lowestCostNode))
                {
                    if (child.gCost > lowestCostNode.gCost)
                    {
                        continue;
                    }

                    openSet.Remove(lowestCostNode);
                    openSet.Add(child);
                    lowestCostSet.Remove(roundedChildHeadingAngle);
                    lowestCostSet.Add(roundedChildHeadingAngle, child);
                }
                else
                {
                    lowestCostSet.Add(roundedChildHeadingAngle, child);
                    openSet.Add(child);
                }
            }
        }

        if (finalNode == null)
        {
            return null;
        }

        var path = RetracePath(finalNode);

        Debug.Log("Hybrid A* path:");

        foreach (var movement in path)
        {
            Debug.Log(movement);
        }

        if (reedsSheppPath != null)
        {
            path.AddRange(reedsSheppPath);

            Debug.Log("Reeds-Shepp path:");

            foreach (var movement in reedsSheppPath)
            {
                Debug.Log(movement);
            }
        }

        return path;
    }

    private List<Node> GetNodeChildren(Node node)
    {
        return ((Movement.Gear[])Enum.GetValues(typeof(Movement.Gear)))
            .SelectMany(gear => ((Movement.Steering[])Enum.GetValues(typeof(Movement.Steering)))
                .Select(steering =>
                {
                    Movement movement = new(Constants.DriveDistance, steering, gear);

                    var childRearWheelPosition =
                        Car.CalculatePositionAfterMovement(node.RearWheelPosition, node.HeadingAngle, movement);

                    if (!_grid.IsPositionWalkable(childRearWheelPosition))
                    {
                        return null;
                    }

                    var childHeadingAngle = Car.CalculateHeadingAfterMovement(node.HeadingAngle, movement);

                    return new Node(childRearWheelPosition, childHeadingAngle, movement, node);
                }))
            .Where(child => child != null)
            .ToList();
    }

    /*
     * Currently, the function is not functioning as expected.
     * The issue lies in the generation of the Reeds-Shepp path for certain cases.
     * Initial investigations reveal that the start and goal positions are correctly defined.
     * However, the generated path differs significantly from expected results.
     * Notably, when utilizing the same start and goal positions with the ReedsShepp class directly,
     * the correct path is generated.
     * The discrepancy arises specifically when employing these positions within the context of the HybridAStar class,
     * resulting in a divergent path output.
     */
    private bool TryAnalyticalExpansion(Vector3 position, float headingAngle, out Path reedsSheppPath)
    {
        ReedsShepp reedsShepp = new();

        var path = reedsShepp.GetOptimalPath(position.x, position.z, headingAngle, _goalCar.RearWheelPosition.x,
            _goalCar.RearWheelPosition.z, _goalCar.HeadingAngle);

        var waypoints = PathBuilder.GenerateWaypoints(position, headingAngle, path, 36);

        for (var i = 0; i < waypoints.Count - 1; i++)
        {
            var start = waypoints[i];
            var end = waypoints[i + 1];

            var waypoint = waypoints[i];

            // this collision check still needs to be improved
            if (!_grid.IsPositionWalkable(waypoint) || Physics.Linecast(start, end, _grid.UnwalkableMask))
            {
                reedsSheppPath = null;

                return false;
            }
        }

        // it was expected that this part would not be necessary
        var lastWaypoint = waypoints.Last();

        var lastWaypointCell = _grid.GetCell(lastWaypoint);
        var goalCell = _grid.GetCell(_goalCar.RearWheelPosition);

        if (lastWaypointCell != goalCell)
        {
            reedsSheppPath = null;

            return false;
        }

        reedsSheppPath = path;
        return true;
    }

    private Path RetracePath(Node node)
    {
        Path path = new();

        var currentNode = node;

        while (currentNode != null)
        {
            path.Add(currentNode.MovementInfo);

            _finalNodes.Add(currentNode);

            currentNode = currentNode.Parent;
        }

        path.Reverse();

        _finalNodes.Reverse();

        return path;
    }

    public void DrawExpandedNodes()
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

    public void DrawFinalNodes()
    {
        if (_finalNodes == null)
        {
            return;
        }

        foreach (var node in _finalNodes)
        {
            if (node.Parent == null)
            {
                continue;
            }

            Gizmos.color = Color.red;

            Gizmos.DrawLine(node.Parent.RearWheelPosition, node.RearWheelPosition);
        }
    }
}