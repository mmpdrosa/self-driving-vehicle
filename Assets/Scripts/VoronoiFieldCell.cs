using System.Collections.Generic;
using UnityEngine;

public class VoronoiFieldCell : Cell
{
    public int Region { get; private set; }

    public bool IsEdge { get; private set; }

    public float ClosestObstacleDistance { get; private set; }

    public float ClosestEdgeDistance { get; private set; }

    public float FieldValue { get; private set; }

    public List<VoronoiFieldCell> ClosestObstacleCells { get; private set; }

    public List<VoronoiFieldCell> ClosestEdgeCells { get; private set; }

    public VoronoiFieldCell(Vector3 center, bool isWalkable, int x, int y) : base(center, isWalkable, x, y)
    {
        Region = -1;
        IsEdge = false;
    }

    public void SetRegion(int region)
    {
        Region = region;
    }

    public void SetIsEdge(bool isEdge)
    {
        IsEdge = isEdge;
    }

    public void SetClosestObstacleDistance(float distance)
    {
        ClosestObstacleDistance = distance;
    }

    public void SetFieldValue(float value)
    {
        FieldValue = value;
    }

    public void SetClosestObstacleCells(List<VoronoiFieldCell> cells)
    {
        ClosestObstacleCells = cells;
    }

    public void SetClosestEdgeDistance(float distance)
    {
        ClosestEdgeDistance = distance;
    }

    public void SetClosestEdgeCells(List<VoronoiFieldCell> cells)
    {
        ClosestEdgeCells = cells;
    }
}