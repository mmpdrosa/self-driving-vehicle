using System.Collections.Generic;
using UnityEngine;
using static PathSegment;
using Path = System.Collections.Generic.List<PathSegment>;

public class PathDrawer : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer;

    private float radius = 5.9f;

    private void Start()
    {

    }

    public void Draw(float x1, float y1, float theta1, float x2, float y2, float theta2, Path path)
    {
        List<Vector3> waypoints = GenerateWaypoints(path, x1, y1, theta1);
        DrawWaypoints(waypoints);
    }

    // organizar essa função depois

    private List<Vector3> GenerateWaypoints(Path path, float x1, float y1, float theta1)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector3 currentPosition = new Vector3(x1, 0, y1);
        float currentHeading = theta1 * Mathf.Deg2Rad;

        waypoints.Add(currentPosition);

        float circumference = 2 * Mathf.PI * 5.9f;

        foreach (PathSegment segment in path)
        {
            Debug.Log(segment);

            switch (segment.steering)
            {

                case Steering.Left:
                    float cx = currentPosition.x - Mathf.Cos(currentHeading) * radius;
                    float cz = currentPosition.z + Mathf.Sin(currentHeading) * radius;

                    float finalAngle = segment.distance * 5.9f / circumference * 360f * Mathf.Deg2Rad;
                    float deltaAngle = finalAngle / 100;

                    for (int i = 0; i < 100; i++)
                    {

                        currentHeading += deltaAngle * (segment.gear == Gear.Forward ? -1 : 1);

                        float x = cx + radius * Mathf.Cos(currentHeading);
                        float z = cz - radius * Mathf.Sin(currentHeading);

                        currentPosition = new Vector3(x, 0, z);
                        waypoints.Add(currentPosition);
                    }

                    continue;

                case Steering.Straight:
                    currentPosition.x += segment.distance * radius * Mathf.Sin(currentHeading) * (segment.gear == Gear.Forward ? 1 : -1);
                    currentPosition.z += segment.distance * radius * Mathf.Cos(currentHeading) * (segment.gear == Gear.Forward ? 1 : -1);
                    waypoints.Add(currentPosition);

                    continue;

                
                case Steering.Right:
                    cx = currentPosition.x + Mathf.Cos(currentHeading) * radius;
                    cz = currentPosition.z - Mathf.Sin(currentHeading) * radius;

                    float finalAngle2 = segment.distance * 5.9f / circumference * 360f * Mathf.Deg2Rad;
                    float deltaAngle2 = finalAngle2 / 10;

                    for (int i = 0; i < 10; i++)
                    {
                        currentHeading += deltaAngle2 * (segment.gear == Gear.Forward ? 1 : -1);

                        float x = cx - radius * Mathf.Cos(currentHeading);
                        float z = cz + radius * Mathf.Sin(currentHeading);

                        currentPosition = new Vector3(x, 0, z);
                        waypoints.Add(currentPosition);
                    }

                    continue;
            }
        }

        return waypoints;
    }


    private void DrawWaypoints(List<Vector3> waypoints)
    {
        lineRenderer.positionCount = waypoints.Count;
        lineRenderer.SetPositions(waypoints.ToArray());
    }
}
