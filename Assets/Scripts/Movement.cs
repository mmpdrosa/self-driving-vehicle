public class Movement
{
    public enum Steering
    {
        Left = -1,
        Straight = 0,
        Right = 1
    };

    public enum Gear
    {
        Backward = -1,
        Forward = 1
    };

    public float Distance { get; set; }
    public Steering SteeringVal { get; set; }
    public Gear GearVal { get; set; }

    public Movement(float distance, Steering steering, Gear gear)
    {
        Distance = distance >= 0 ? distance : -distance;
        SteeringVal = steering;
        GearVal = distance >= 0 ? gear : (Gear)(-(int)gear);
    }

    public override string ToString()
    {
        return $"Distance: {Distance}, Steering: {SteeringVal}, Gear: {GearVal}";
    }

    public void ReverseSteering()
    {
        SteeringVal = (Steering)(-(int)SteeringVal);
    }

    public void ReverseGear()
    {
        GearVal = (Gear)(-(int)GearVal);
    }
}