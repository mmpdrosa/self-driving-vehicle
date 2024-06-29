using UnityEngine;

public class Cell
{
    public Vector3 Center;
    public bool IsWalkable;
    public Vector2Int GridPosition;

    public Cell(Vector3 center, bool isWalkable, int x, int y)
    {
        Center = center;
        IsWalkable = isWalkable;
        GridPosition = new Vector2Int(x, y);
    }
}