using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Path = System.Collections.Generic.List<Movement>;

public class HybridAStar
{
    public static Path FindPath(Grid<Cell> grid, Car startCar, Car goalCar, float[,] euclideanCosts,
        float[,] holonomicCosts, float[,] flowFieldCosts, float[,] voronoiFieldCosts, out List<Node> finalNodes,
        out List<Node> expandedNodes, bool analyticalExpansion = true)
    {
        // TODO: Validate start and goal positions

        finalNodes = new List<Node>();
        expandedNodes = new List<Node>();

        var openSet = new Heap<Node>(Constants.Iterations * 6);

        var lowestCostSetList = new Dictionary<int, Node>[grid.Width, grid.Height];

        for (var i = 0; i < grid.Width; i++)
        {
            for (var j = 0; j < grid.Height; j++)
            {
                lowestCostSetList[i, j] = new Dictionary<int, Node>();
            }
        }

        var startMovement = new Movement(0, Movement.Steering.Straight, Movement.Gear.Forward);

        var startNode = new Node(startCar.RearWheelPosition, startCar.HeadingAngle, startMovement, parent: null);

        openSet.Add(startNode);

        Path reedsSheppPath = null;

        Node finalNode = null;

        var iterations = 0;

        while (iterations < Constants.Iterations)
        {
            iterations++;

            var currentNode = openSet.RemoveFirst();

            // TODO: Run the analytical expansion only when the goal is close to the current node
            if (analyticalExpansion && TryAnalyticalExpansion(grid, currentNode.RearWheelPosition,
                    currentNode.HeadingAngle, goalCar.RearWheelPosition, goalCar.HeadingAngle,
                    out reedsSheppPath))
            {
                finalNode = currentNode;
                break;
            }

            var distance = (currentNode.RearWheelPosition - goalCar.RearWheelPosition).magnitude;

            var goalPositionReached = distance < Constants.GoalDistanceThreshold;

            var goalAngleReached = Math.Abs(currentNode.HeadingAngle - goalCar.HeadingAngle) <
                                   Constants.GoalHeadingAngleThreshold;

            var goalReached = goalPositionReached && goalAngleReached;

            if (goalReached)
            {
                finalNode = currentNode;

                break;
            }

            var children = GetNodeChildren(grid, currentNode);

            expandedNodes.Add(currentNode);

            foreach (var child in children)
            {
                if (!grid.TryGetCellFromWorldPosition(child.RearWheelPosition, out var childCell)) continue;

                //child.hCost = childCell.EuclideanDistance + childCell.HolonomicHeuristic;

                var x = childCell.GridPosition.x;
                var y = childCell.GridPosition.y;

                // TODO: Make the weights configurable to allow for better tuning
                child.hCost = euclideanCosts[x, y] + holonomicCosts[x, y] + 1.5f * flowFieldCosts[x, y] +
                              15f * (-1f + voronoiFieldCosts[x, y]);

                var roundedChildHeadingAngle =
                    Utils.RoundValueToNearestStep(child.HeadingAngle, Constants.HeadingAngleStep);

                var lowestCostSet = lowestCostSetList[childCell.GridPosition.x, childCell.GridPosition.y];

                if (lowestCostSet.TryGetValue(roundedChildHeadingAngle, out var lowestCostNode))
                {
                    if (child.gCost > lowestCostNode.gCost) continue;

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

        if (finalNode == null) return null;

        var path = RetracePath(finalNode, finalNodes);

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

    private static List<Node> GetNodeChildren(Grid<Cell> grid, Node node)
    {
        return ((Movement.Gear[])Enum.GetValues(typeof(Movement.Gear)))
            .SelectMany(gear => ((Movement.Steering[])Enum.GetValues(typeof(Movement.Steering)))
                .Select(steering =>
                {
                    var movement = new Movement(Constants.DriveDistance, steering, gear);

                    var childRearWheelPosition =
                        Car.CalculatePositionAfterMovement(node.RearWheelPosition, node.HeadingAngle, movement);

                    if (!grid.IsPositionWalkable(childRearWheelPosition))
                    {
                        return null;
                    }

                    var childHeadingAngle = Car.CalculateHeadingAfterMovement(node.HeadingAngle, movement);

                    return new Node(childRearWheelPosition, childHeadingAngle, movement, node);
                }))
            .Where(child => child != null)
            .ToList();
    }

    private static bool TryAnalyticalExpansion(Grid<Cell> grid, Vector3 currentPosition, float currentHeadingAngle,
        Vector3 goalPosition, float goalHeadingAngle, out Path reedsSheppPath)
    {
        var path = ReedsShepp.GetOptimalPath(currentPosition.x, currentPosition.z, currentHeadingAngle, goalPosition.x,
            goalPosition.z, goalHeadingAngle);

        var waypoints = PathBuilder.GenerateWaypoints(currentPosition, currentHeadingAngle, path, 36);

        for (var i = 0; i < waypoints.Count - 1; i++)
        {
            var start = waypoints[i];
            var end = waypoints[i + 1];

            var waypoint = waypoints[i];

            // TODO: Implement a more accurate collision check
            if (!grid.IsPositionWalkable(waypoint) || Physics.Linecast(start, end, grid.UnwalkableMask))
            {
                reedsSheppPath = null;

                return false;
            }
        }

        reedsSheppPath = path;
        return true;
    }

    private static Path RetracePath(Node node, List<Node> finalNodes)
    {
        Path path = new();

        var currentNode = node;

        while (currentNode != null)
        {
            path.Add(currentNode.MovementInfo);

            finalNodes.Add(currentNode);

            currentNode = currentNode.Parent;
        }

        path.Reverse();

        finalNodes.Reverse();

        return path;
    }
}