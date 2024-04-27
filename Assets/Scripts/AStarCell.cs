using UnityEngine;

public class AStarCell : IHeapItem<AStarCell>
{
    public Vector3 position;
    public bool walkable;
    public float gCost;
    public float hCost;
    public int heapIndex;
    public Vector2Int gridPosition;
    public AStarCell parent;

    public AStarCell(Vector3 _position, bool _walkable, Vector2Int _gridPosition)
    {
        position = _position;
        gridPosition = _gridPosition;
        walkable = _walkable;
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

    public int CompareTo(AStarCell other)
    {
        int compare = fCost.CompareTo(other.fCost);

        if (compare == 0)
        {
            compare = hCost.CompareTo(other.hCost);
        }

        return -compare;
    }
}
