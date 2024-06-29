using System.Collections.Generic;
using UnityEngine;

public class FlowFieldCell : Cell
{
    public float Cost { get; private set; }

    public int Region { get; private set; }

    public HashSet<FlowFieldCell> Neighbors { get; private set; } = new();

    public HashSet<FlowFieldCell> ClosestStartCells { get; } = new();

    public FlowFieldCell(Vector3 center, bool isWalkable, int x, int y) : base(center, isWalkable, x, y)
    {
        Region = -1;
    }

    public void SetCost(float cost)
    {
        Cost = cost;
    }

    public void SetRegion(int region)
    {
        Region = region;
    }

    public void SetNeighbors(HashSet<FlowFieldCell> neighbors)
    {
        Neighbors = neighbors;
    }

    public void AddClosestStartCell(FlowFieldCell cell)
    {
        ClosestStartCells.Add(cell);
    }

    public void ClearClosestStartCells()
    {
        ClosestStartCells.Clear();
    }

    public void Reset()
    {
        Cost = float.MaxValue;
        Neighbors.Clear();
        ClosestStartCells.Clear();
    }
}