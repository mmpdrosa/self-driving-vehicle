using UnityEngine;

public class Utils
{
    public static readonly Vector2Int[] DirectionsWithCorners =
    {
        new(-1, -1), new(0, -1), new(1, -1), new(-1, 0), new(1, 0), new(-1, 1), new(0, 1), new(1, 1)
    };

    public static readonly Vector2Int[] DirectionsWithoutCorners =
    {
        new(0, -1), new(-1, 0), new(1, 0), new(0, 1)
    };

    public static readonly Vector2Int[] CornerDirections =
    {
        new(-1, -1), new(1, -1), new(-1, 1), new(1, 1)
    };

    public static float WrapAngle(float angle)
    {
        angle %= 360;

        return angle;
    }

    public static int RoundValueToNearestStep(float value, float stepSize)
    {
        var roundedValue = Mathf.RoundToInt(value / stepSize) * stepSize;
        return (int)roundedValue;
    }
}