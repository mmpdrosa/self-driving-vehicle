using System.Collections.Generic;
using UnityEngine;
using Path = System.Collections.Generic.List<Movement>;

public class PathBuilder
{
    public static List<Vector3> GenerateWaypoints(Vector3 currentPosition, float currentHeading, Path path,
        int step = 10)
    {
        List<Vector3> waypoints = new();

        currentHeading *= Mathf.Deg2Rad;

        foreach (var movement in path)
        {
            switch (movement.SteeringVal)
            {
                case Movement.Steering.Left:
                    var cx = currentPosition.x - Mathf.Cos(currentHeading) * Constants.CarTurningRadius;
                    var cz = currentPosition.z + Mathf.Sin(currentHeading) * Constants.CarTurningRadius;

                    var finalAngle = movement.Distance /
                                     Constants.CarTurningCircumference *
                                     360f *
                                     Mathf.Deg2Rad;
                    var deltaAngle = finalAngle / step;

                    for (var i = 0; i < step; i++)
                    {
                        currentHeading += deltaAngle * (movement.GearVal == Movement.Gear.Forward ? -1 : 1);

                        var x = cx + Constants.CarTurningRadius * Mathf.Cos(currentHeading);
                        var z = cz - Constants.CarTurningRadius * Mathf.Sin(currentHeading);

                        currentPosition = new Vector3(x, currentPosition.y, z);
                        waypoints.Add(currentPosition);
                    }

                    break;

                case Movement.Steering.Straight:
                    currentPosition.x += movement.Distance * Mathf.Sin(currentHeading) *
                                         (movement.GearVal == Movement.Gear.Forward ? 1 : -1);
                    currentPosition.z += movement.Distance * Mathf.Cos(currentHeading) *
                                         (movement.GearVal == Movement.Gear.Forward ? 1 : -1);
                    waypoints.Add(currentPosition);

                    break;


                case Movement.Steering.Right:
                    cx = currentPosition.x + Mathf.Cos(currentHeading) * Constants.CarTurningRadius;
                    cz = currentPosition.z - Mathf.Sin(currentHeading) * Constants.CarTurningRadius;

                    var finalAngle2 = movement.Distance /
                                      Constants.CarTurningCircumference *
                                      360f *
                                      Mathf.Deg2Rad;
                    var deltaAngle2 = finalAngle2 / step;

                    for (var i = 0; i < step; i++)
                    {
                        currentHeading += deltaAngle2 * (movement.GearVal == Movement.Gear.Forward ? 1 : -1);

                        var x = cx - Constants.CarTurningRadius * Mathf.Cos(currentHeading);
                        var z = cz + Constants.CarTurningRadius * Mathf.Sin(currentHeading);

                        currentPosition = new Vector3(x, currentPosition.y, z);
                        waypoints.Add(currentPosition);
                    }

                    break;
            }
        }

        return waypoints;
    }
}