using UnityEngine;

public class Utils
{
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