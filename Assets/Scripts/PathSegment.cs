public class PathSegment
{
    public enum Steering { Left = -1, Straight = 0, Right = 1 };
    public enum Gear { Backward = -1, Forward = 1 };

    public Steering steering;
    public Gear gear;
    public float distance;

    public PathSegment(float _distance, Steering _steering, Gear _gear)
    {
        distance = _distance >= 0 ? _distance : -_distance;
        steering = _steering;
        gear =  _distance >= 0 ? _gear : (Gear)(-(int)_gear);
    }

    public override string ToString()
    {
        return $"{{Steering: {steering}\tGear: {gear}\tDistance: {distance}}}";
    }

    public void ReverseSteering()
    {
        steering = (Steering)(-(int)steering);
    }

    public void ReverseGear()
    {
        gear = (Gear)(-(int)gear);  
    }
}
