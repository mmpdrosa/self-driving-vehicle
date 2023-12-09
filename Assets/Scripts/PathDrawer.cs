using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static PathSegment;

using Path = System.Collections.Generic.List<PathSegment>;

public class PathDrawer : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer;

    private void Start()
    {

    }

    public void Draw(float x1, float y1, float theta1, float x2, float y2, float theta2, Path path)
    {
        List<Vector3> waypoints = GenerateWaypoints(path, x1, y1, theta1, x2, y2, theta2);
        DrawWaypoints(waypoints);
    }

    private List<Vector3> GenerateWaypoints(Path path, float x1, float y1, float theta1, float x2, float y2, float theta2)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector3 currentPosition = new Vector3(x1, 0, y1);
        float currentHeading = theta1 * Mathf.Deg2Rad;

        waypoints.Add(currentPosition);

        float C = 2 * Mathf.PI * 5.9f;

        foreach (PathSegment segment in path)
        {
            Debug.Log(segment);
            float side = segment.steering == Steering.Left ? 1 : -1;
            float direction = segment.gear == Gear.Forward ? 1 : -1;

            if (segment.steering != Steering.Straight)
            {
                float ratio = segment.distance * 5.9f * direction / C;
                float turningAngle = 360 * ratio * Mathf.Deg2Rad;

                int numSegments = 100;
                float smallTurningAngle = turningAngle / numSegments;

                for (int i = 0; i < numSegments; i++)
                {
                    float x_dist = Mathf.Cos(currentHeading) * 5.9f * side;
                    float y_dist = Mathf.Sin(currentHeading) * 5.9f * side;

                    float center_x = currentPosition.x - x_dist;
                    float center_y = currentPosition.z - y_dist;

                    float x_dist_new = Mathf.Cos(currentHeading + smallTurningAngle * side) * 5.9f * side;
                    float y_dist_new = Mathf.Sin(currentHeading + smallTurningAngle * side) * 5.9f * side;

                    float x_new = center_x + x_dist_new;
                    float y_new = center_y + y_dist_new;

                    currentPosition.x = x_new;
                    currentPosition.z = y_new;
                    currentHeading += smallTurningAngle * side;

                    waypoints.Add(new Vector3(currentPosition.x, 0, currentPosition.z));
                    // DrawCircle(new Vector3(center_x, 0, center_y), 5.9f);
                }
            }
            else
            {
                currentPosition.x = currentPosition.x - segment.distance * 5.9f * direction * Mathf.Sin(currentHeading);
                currentPosition.z = currentPosition.z + segment.distance * 5.9f * direction * Mathf.Cos(currentHeading);

                waypoints.Add(new Vector3(currentPosition.x, 0, currentPosition.z));
            }

            Debug.Log(currentPosition);
        }

        // waypoints.Add(new Vector3(x2, 0, y2));

        return waypoints;
    }


    private void DrawWaypoints(List<Vector3> waypoints)
    {
        lineRenderer.positionCount = waypoints.Count;
        lineRenderer.SetPositions(waypoints.ToArray());
    }

    void DrawCircle(Vector3 center, float radius)
    {
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        lineRenderer.widthMultiplier = 0.1f; // Ajuste a largura da linha conforme necessário

        lineRenderer.positionCount = 64 + 1;
        lineRenderer.useWorldSpace = true; // Agora estamos usando coordenadas de mundo

        float deltaTheta = (2f * Mathf.PI) / 64;
        float theta = 0f;

        for (int i = 0; i < 64 + 1; i++)
        {
            float x = center.x + radius * Mathf.Cos(theta);
            float z = center.z + radius * Mathf.Sin(theta);

            lineRenderer.SetPosition(i, new Vector3(x, center.y, z));

            theta += deltaTheta;
        }
    }
}
