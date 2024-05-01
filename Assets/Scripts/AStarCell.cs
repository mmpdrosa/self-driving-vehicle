using UnityEngine;

public class AStarCell : IHeapItem<AStarCell>
{
    public Vector2Int GridPosition { get; set; }
    public Vector3 Position { get; set; }
    public bool Walkable { get; set; }
    public AStarCell Parent { get; set; }

    public float EuclideanDistance { get; set; }

    public float gCost { get; set; }
    public float hCost { get; set; }
    private float fCost => gCost + hCost;

    public float HolonomicHeuristic => fCost;

    public int HeapIndex { get; set; }

    public AStarCell(Vector2Int gridPosition, Vector3 position, bool walkable)
    {
        GridPosition = gridPosition;
        Position = position;
        Walkable = walkable;
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