using System.Collections.Generic;
using UnityEngine;

public class HybridAStar : MonoBehaviour
{
    private static float driveDistance = Mathf.Sqrt(Mathf.Pow(0.5f, 2) * 2f) + 0.01f;
    private static float[] driveDistances = new float[] { -driveDistance, driveDistance };

    private static float maxSteeringAngle = 40f;
    private static float[] steeringAngles = new float[] { -maxSteeringAngle, 0f, maxSteeringAngle };

    private float positionAccuracy = 1f;
    private float angleAccuracy = 10f;

    private float headingAngleResolution = 15f;

    private Grid grid;

    public Car startCar, endCar;

    private List<Node> finalPath;

    List<Node> expandedNodes;

    LineRenderer lineRenderer;

    void Start()
    {
        grid = GetComponent<Grid>();
        lineRenderer = GetComponent<LineRenderer>();

        for (int i = 0; i < grid.width; i++)
        {
            for (int j = 0; j < grid.height; j++)
            {
                Cell currentCell = grid.data[i, j];

                currentCell.heuristics = (endCar.rearWheelPosition - currentCell.position).magnitude;
            }
        }

        expandedNodes = new List<Node>();

        FindPath(grid, startCar, endCar);
    }

    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        DisplaySearchTree();

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

    private void FindPath(Grid grid, Car startCar, Car endCar)
    {
        Heap<Node> openSet = new Heap<Node>(100000);
        HashSet<int>[,] analyzedCellHeadingAnglesGrid = new HashSet<int>[grid.width, grid.height];
        Dictionary<int, Node>[,] lowestCostNodeByAngleGrid = new Dictionary<int, Node>[grid.width, grid.height];


        for (int i = 0; i < grid.width;  i++)
        {
            for (int j = 0; j < grid.height; j++)
            {
                analyzedCellHeadingAnglesGrid[i, j] = new HashSet<int>();
                lowestCostNodeByAngleGrid[i, j] = new Dictionary<int, Node>();
            }
        }

        IntVector2 startCell = grid.GetCellFromWorldPosition(startCar.rearWheelPosition);

        Node startNode = new Node(
            parent: null,
            rearWheelPosition: startCar.rearWheelPosition,
            headingAngle: startCar.headingAngle,
            isReversing: false
        );

        startNode.AddCosts(
            gCost: 0f,
            hCost: grid.data[startCell.x, startCell.y].heuristics
        );

        openSet.Add(startNode);

        IntVector2 goalCell = grid.GetCellFromWorldPosition(endCar.rearWheelPosition);

        Node finalNode = null;

        int flag = 0;

        while(flag < 8000)
        {
            flag++;

            Node currentNode = openSet.RemoveFirst();

            IntVector2 currentCell = grid.GetCellFromWorldPosition(currentNode.rearWheelPosition);

            int roundedHeadingAngle = RoundValueByStep(currentNode.headingAgle, headingAngleResolution);

            HashSet<int> analyzedCellHeadingAngles = analyzedCellHeadingAnglesGrid[currentCell.x, currentCell.y];

            if (!analyzedCellHeadingAngles.Contains(roundedHeadingAngle))
            {
                analyzedCellHeadingAngles.Add(roundedHeadingAngle);
            } else
            {
                continue;
            }

            expandedNodes.Add(currentNode);

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
                IntVector2 childCell = grid.GetCellFromWorldPosition(child.rearWheelPosition);

                int roundedChildHeadingAngle = RoundValueByStep(child.headingAgle, headingAngleResolution);

                HashSet<int> analyzedChildCellHeadingAngles = analyzedCellHeadingAnglesGrid[childCell.x, childCell.y];

                if (analyzedChildCellHeadingAngles.Contains(roundedChildHeadingAngle))
                {
                    continue;
                }

                float childCost = child.gCost;

                Dictionary<int, Node> lowestCostNodeByAngle = lowestCostNodeByAngleGrid[childCell.x, childCell.y];

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

        for (int i = 0; i < driveDistances.Length; i++) 
        {
            float driveDistance = driveDistances[i];

            for (int j = 0; j < steeringAngles.Length; j++)
            {
                float steeringAngle = steeringAngles[j];

                Vector3 childRearWheelPosition = Car.CalculatePositionAfterMovement(node.rearWheelPosition, node.headingAgle, driveDistance, steeringAngle);

                float childHeadingAngle = Car.CalculateHeadingAngleAfterMovement(node.headingAgle, driveDistance, steeringAngle);

                IntVector2 childCell = grid.GetCellFromWorldPosition(childRearWheelPosition);

                if(!grid.IsCellValid(childCell))
                {
                    continue;
                }

                Node childNode = new Node(
                    parent: node,
                    rearWheelPosition: childRearWheelPosition,
                    headingAngle: childHeadingAngle,
                    isReversing: driveDistance < 0f ? true : false
                );

                float gCost = GetCostToReachNode(childNode);
                float hCost = grid.data[childCell.x, childCell.y].heuristics;

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
        // float costSoFar = parent.gCost;

        // Cost 1
        float distanceCost = (node.rearWheelPosition - parent.rearWheelPosition).magnitude;

        // Cost 2 - Voronoi Cost

        // Cost 3
        float reverseCost = node.isReversing ? 2f : 0f;

        // Cost 4
        float switchMotionCost = 0f;

        if ((node.isReversing && !parent.isReversing) || (!node.isReversing && parent.isReversing))
        {
            switchMotionCost = 0.5f;
        }

        return parent.gCost + distanceCost * (1f + reverseCost) + switchMotionCost;
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
        if (expandedNodes != null)
        {
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
}
