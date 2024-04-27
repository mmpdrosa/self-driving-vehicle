using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PathSegment;

public class HybridAStar : MonoBehaviour
{
    private static float driveDistance = Mathf.Sqrt(Mathf.Pow(0.5f, 2) * 2f) + 0.01f;

    private float positionAccuracy = 0.5f; // meters
    private float angleAccuracy = 5f; // degrees

    private float headingAngleResolution = 15f; // degrees

    private List<Node> finalPath;

    List<Node> expandedNodes;

    private AStartGrid grid;

    private void OnDrawGizmos()
    {
        //DisplaySearchTree();

        Gizmos.color = Color.red;

        if (finalPath != null)
        {
            for (int i = 0; i < finalPath.Count; i++)
            {
                Node currentNode = finalPath[i];

                if (currentNode.parent == null)
                {
                    continue;
                }

                Gizmos.DrawLine(currentNode.rearWheelPosition, currentNode.parent.rearWheelPosition);
            }
        }
    }

    public void FindPath(AStartGrid _grid, Car startCar, Car endCar)
    {
        grid = _grid;

        Heap<Node> openSet = new Heap<Node>(100000);
        HashSet<int>[,] analyzedCellHeadingAnglesGrid = new HashSet<int>[grid.width, grid.height];
        Dictionary<int, Node>[,] lowestCostNodeByAngleGrid = new Dictionary<int, Node>[grid.width, grid.height];

        expandedNodes = new List<Node>();

        for (int i = 0; i < grid.width;  i++)
        {
            for (int j = 0; j < grid.height; j++)
            {
                analyzedCellHeadingAnglesGrid[i, j] = new HashSet<int>();
                lowestCostNodeByAngleGrid[i, j] = new Dictionary<int, Node>();
            }
        }

        AStarCell startCell = grid.GetCellFromWorldPosition(startCar.rearWheelPosition);

        if (startCell == null)
        {
            return;
        }

        Node startNode = new Node(
            parent: null,
            rearWheelPosition: startCar.rearWheelPosition,
            headingAngle: startCar.headingAngle,
            isReversing: false
        );

        startNode.AddCosts(
            gCost: 0f,
            hCost: startCell.fCost
        );

        openSet.Add(startNode);

        AStarCell goalCell = grid.GetCellFromWorldPosition(endCar.rearWheelPosition);

        if (goalCell == null)
        {
            return;
        }

        Node finalNode = null;

        int flag = 0;

        while(flag < 100000)
        {
            flag++;

            Node currentNode = openSet.RemoveFirst();

            AStarCell currentCell = grid.GetCellFromWorldPosition(currentNode.rearWheelPosition);

            if (currentCell == null)
            {
                continue;
            }

            int roundedHeadingAngle = RoundValueByStep(currentNode.headingAgle, headingAngleResolution);

            HashSet<int> analyzedCellHeadingAngles = analyzedCellHeadingAnglesGrid[currentCell.gridPosition.x, currentCell.gridPosition.y];

            if (!analyzedCellHeadingAngles.Contains(roundedHeadingAngle))
            {
                analyzedCellHeadingAngles.Add(roundedHeadingAngle);
            } else
            {
                continue;
            }

            expandedNodes.Add(currentNode);

            // yield return new WaitForSeconds(0.001f);

            float distanceToGoal = (currentNode.rearWheelPosition - endCar.rearWheelPosition).sqrMagnitude;

            float headingAngleDifference = Mathf.Abs(endCar.headingAngle - currentNode.headingAgle);

            if ((distanceToGoal < Mathf.Pow(positionAccuracy, 2) || currentCell == goalCell) && headingAngleDifference < angleAccuracy)
            {
                finalNode = currentNode;

                break;
            }

            List<Node> children = GetNodeChildren(currentNode);

            foreach ( Node child in children )
            {
                AStarCell childCell = grid.GetCellFromWorldPosition(child.rearWheelPosition);

                if (childCell == null)
                {
                    continue;
                }

                int roundedChildHeadingAngle = RoundValueByStep(child.headingAgle, headingAngleResolution);

                HashSet<int> analyzedChildCellHeadingAngles = analyzedCellHeadingAnglesGrid[childCell.gridPosition.x, childCell.gridPosition.y];

                if (analyzedChildCellHeadingAngles.Contains(roundedChildHeadingAngle))
                {
                    continue;
                }

                float childCost = child.gCost;

                Dictionary<int, Node> lowestCostNodeByAngle = lowestCostNodeByAngleGrid[childCell.gridPosition.x, childCell.gridPosition.y];

                if (lowestCostNodeByAngle.ContainsKey(roundedChildHeadingAngle))
                {
                    Node currentLowestCostNode = lowestCostNodeByAngle[roundedChildHeadingAngle];

                    // atualizar o nó de menor custo nesse ângulo 
                    if (childCost < currentLowestCostNode.gCost)
                    {
                        currentLowestCostNode.CopyData(child);

                        openSet.UpdateItem(currentLowestCostNode);
                    }

                    continue;
                } else
                {
                    lowestCostNodeByAngle[roundedChildHeadingAngle] = child;
                }

                openSet.Add(child);
            }
        }

        List<Node> path = RetracePath(finalNode);

        finalPath = path;
    }


    public static int RoundValueByStep(float value, float step)
    {
        return (int)(Mathf.RoundToInt(value / step) * step);
    }

    

    public List<Node> GetNodeChildren(Node node)
    {
        List<Node> children = new List<Node>();

        foreach (Gear gear in System.Enum.GetValues(typeof(Gear)))
        {
            foreach (Steering steering in System.Enum.GetValues(typeof(Steering)))
            {
                PathSegment pathSegment = new PathSegment(driveDistance, steering, gear);

                Vector3 childRearWheelPosition = Car.CalculatePositionAfterMovement(node.rearWheelPosition, node.headingAgle, pathSegment);

                float childHeadingAngle = Car.CalculateHeadingAngleAfterMovement(node.headingAgle, pathSegment);

                if (!grid.IsPositionWalkable(childRearWheelPosition))
                {
                    continue;
                }

                AStarCell childCell = grid.GetCellFromWorldPosition(childRearWheelPosition);

                Node childNode = new Node(
                    parent: node,
                    rearWheelPosition: childRearWheelPosition,
                    headingAngle: childHeadingAngle,
                    isReversing: gear == Gear.Backward
                );

                float gCost = GetCostToReachNode(childNode);

                // wheel cost
                // gCost += steeringAngle != 0 ? 0.2f : 0;


                float hCost = childCell.fCost;

                childNode.AddCosts(gCost, hCost);

                children.Add(childNode);
            }
        }

        return children;
    }

    private static float GetCostToReachNode(Node node)
    {
        Node parent = node.parent;

        // Cost 0
        float costSoFar = parent.gCost;

        // Cost 1
        float distanceCost = (node.rearWheelPosition - parent.rearWheelPosition).magnitude;

        float reverseCost = node.isReversing ? 1.5f : 0f;

        float switchMotionCost = 0f;

        if ((node.isReversing && !parent.isReversing) || (!node.isReversing && parent.isReversing))
        {
            switchMotionCost = 0.5f;
        }

        return costSoFar + distanceCost + reverseCost + switchMotionCost;
    }

    private static List<Node> RetracePath(Node finalNode)
    {
        List<Node> path = new List<Node>();

        Node currentNode = finalNode;

        while (currentNode != null)
        {
            path.Add(currentNode);

            currentNode = currentNode.parent;
        }

        path.Reverse();

        return path;
    }

    public void DisplaySearchTree()
    {
        if (expandedNodes == null)
        {
            return;
        }

        for (int i = 0; i < expandedNodes.Count; i++)
        {
            Node currentNode = expandedNodes[i];

            if (currentNode.parent == null)
            {
                continue;
            }

            Gizmos.color = currentNode.isReversing ? Color.cyan : Color.green;

            Gizmos.DrawLine(currentNode.rearWheelPosition, currentNode.parent.rearWheelPosition);
        }
    }
}
