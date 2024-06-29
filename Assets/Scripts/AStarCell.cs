using UnityEngine;

public class AStarCell : Cell, IHeapItem<AStarCell>
{
    public float gCost { get; private set; }

    public float hCost { get; private set; }

    public float fCost => gCost + hCost;

    public AStarCell Parent { get; private set; }

    public int HeapIndex { get; set; }

    public AStarCell(Vector3 center, bool isWalkable, int x, int y) : base(center, isWalkable, x, y)
    {
    }

    public AStarCell(AStarCell other) : base(other.Center, other.IsWalkable, other.GridPosition.x, other.GridPosition.y)
    {
        Center = other.Center;
        IsWalkable = other.IsWalkable;
        gCost = other.gCost;
        hCost = other.hCost;
        Parent = other.Parent;
    }

    public void SetCosts(float gCost, float hCost)
    {
        this.gCost = gCost;
        this.hCost = hCost;
    }

    public void SetParent(AStarCell parent)
    {
        Parent = parent;
    }

    public void Reset()
    {
        Parent = null;
        gCost = 0;
        hCost = 0;
    }

    public int CompareTo(AStarCell other)
    {
        var compare = fCost.CompareTo(other.fCost);

        if (compare == 0)
        {
            compare = hCost.CompareTo(other.hCost);
        }

        return -compare;
    }
}