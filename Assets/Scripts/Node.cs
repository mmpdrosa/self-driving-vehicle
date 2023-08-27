using UnityEngine;

public class Node : IHeapItem<Node>
{
    public float gCost;
    public float hCost;

    public Node parent;
    public Vector3 rearWheelPosition;
    public float headingAgle;
    public bool isReversing;

    public int heapIndex;

    public Node(Node parent, Vector3 rearWheelPosition, float headingAngle, bool isReversing)
    {
        this.parent = parent;
        this.rearWheelPosition = rearWheelPosition;
        this.headingAgle = headingAngle;
        this.isReversing = isReversing;
    }

    public void AddCosts(float gCost, float hCost)
    {
        this.gCost = gCost;
        this.hCost = hCost;
    }

    public void CopyData(Node other)
    {
        gCost = other.gCost;
        hCost = other.hCost;
        parent = other.parent;
        rearWheelPosition = other.rearWheelPosition;
        headingAgle = other.headingAgle;
        isReversing = other.isReversing;
    }

    public float fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(Node other)
    {
        int compare = fCost.CompareTo(other.fCost);

        if (compare == 0)
        {
            compare = hCost.CompareTo(other.hCost);
        }

        return -compare;
    }
}
