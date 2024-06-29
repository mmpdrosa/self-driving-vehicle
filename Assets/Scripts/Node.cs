using UnityEngine;

public class Node : IHeapItem<Node>
{
    public Vector3 RearWheelPosition { get; set; }

    public float HeadingAngle { get; set; }

    public Movement MovementInfo { get; set; }

    public Node Parent { get; set; }

    public float ChangingDirectionCost =>
        Parent != null && Parent.MovementInfo.GearVal != MovementInfo.GearVal ? Constants.ChangingDirectionCost : 0;

    public float IsReversingCost => MovementInfo.GearVal == Movement.Gear.Backward ? Constants.ReversingCost : 0;

    public float IsTurningCost => MovementInfo.SteeringVal != Movement.Steering.Straight ? Constants.TurningCost : 0;

    public float ExpansionCost => MovementInfo.Distance + ChangingDirectionCost + IsReversingCost + IsTurningCost;

    public float gCost => Parent != null ? Parent.gCost + ExpansionCost : 0;

    public float hCost { get; set; }

    public float fCost => gCost + hCost;

    public int HeapIndex { get; set; }

    public Node(Vector3 rearWheelPosition, float headingAngle, Movement movement, Node parent)
    {
        RearWheelPosition = rearWheelPosition;
        HeadingAngle = headingAngle;
        MovementInfo = movement;
        Parent = parent;
    }

    public int CompareTo(Node other)
    {
        var compare = fCost.CompareTo(other.fCost);

        if (compare == 0)
        {
            compare = hCost.CompareTo(other.hCost);
        }

        return -compare;
    }
}