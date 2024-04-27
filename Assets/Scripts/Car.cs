using UnityEngine;
using static PathSegment;
using static UnityEngine.Rendering.HableCurve;

public class Car : MonoBehaviour
{
    public Vector3 rearWheelPosition;
    public float headingAngle;

    public static float lenght = 5.021f;
    public static float width = 2.189f;
    public static float height = 1.431f;
    public static float wheelBase = 2.96f;
    public static float turningRadius = 5.9f;

    private void Awake()
    {
        headingAngle = transform.eulerAngles.y;
        rearWheelPosition = transform.position + transform.rotation * new Vector3(0f, 0f, -1.43f);
    }

    void Start()
    {

    }   
    void Update()
    {
        headingAngle = transform.eulerAngles.y;
        rearWheelPosition = transform.position + transform.rotation * new Vector3(0f, 0f, -1.43f);

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        Quaternion carRotation = transform.rotation;
        Vector3 halfExtents = new Vector3(width / 2, height / 2, lenght / 2);
        Vector3 center = transform.position + carRotation * new Vector3(0f, halfExtents.y, 0f);

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(center, carRotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2);

        Gizmos.matrix = oldMatrix;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(rearWheelPosition, 0.1f); 
    }


    public static Vector3 CalculatePositionAfterMovement(Vector3 rearWheelPosition, float headingAgle, PathSegment pathSegment)
    {
        headingAgle *= Mathf.Deg2Rad;

        Vector3 newRearWheelPos = Vector3.zero;

        float circumference = 2 * Mathf.PI * turningRadius;

        switch (pathSegment.steering)
        {
            case Steering.Left:
                float cx = rearWheelPosition.x - Mathf.Cos(headingAgle) * turningRadius;
                float cz = rearWheelPosition.z + Mathf.Sin(headingAgle) * turningRadius;

                float finalAngle = pathSegment.distance / circumference * 360f * Mathf.Deg2Rad;

                headingAgle+= finalAngle * (pathSegment.gear == Gear.Forward ? -1 : 1);

                float x = cx + turningRadius * Mathf.Cos(headingAgle);
                float z = cz - turningRadius * Mathf.Sin(headingAgle);

                newRearWheelPos = new Vector3(x, 0, z);
                break;

            case Steering.Straight:
                newRearWheelPos.x = rearWheelPosition.x + pathSegment.distance * Mathf.Sin(headingAgle) * (pathSegment.gear == Gear.Forward ? 1 : -1);
                newRearWheelPos.z = rearWheelPosition.z + pathSegment.distance * Mathf.Cos(headingAgle) * (pathSegment.gear == Gear.Forward ? 1 : -1);
                break;

            case Steering.Right:
                float cx2 = rearWheelPosition.x + Mathf.Cos(headingAgle) * turningRadius;
                float cz2 = rearWheelPosition.z - Mathf.Sin(headingAgle) * turningRadius;

                float finalAngle2 = pathSegment.distance / circumference * 360f * Mathf.Deg2Rad;

                headingAgle+= finalAngle2 * (pathSegment.gear == Gear.Forward ? 1 : -1);

                float x2 = cx2 - turningRadius * Mathf.Cos(headingAgle);
                float z2 = cz2 + turningRadius * Mathf.Sin(headingAgle);

                newRearWheelPos = new Vector3(x2, 0, z2);
                break;
        }

        return newRearWheelPos;
    }


    public static float CalculateHeadingAngleAfterMovement(float headingAgle, PathSegment pathSegment)
    {
        headingAgle *= Mathf.Deg2Rad;

        float circumference = 2 * Mathf.PI * turningRadius; 

        switch (pathSegment.steering)
        {
            case Steering.Left:
                float finalAngle = pathSegment.distance / circumference * 360f * Mathf.Deg2Rad;
                headingAgle += finalAngle * (pathSegment.gear == Gear.Forward ? -1 : 1);

                break;

            case Steering.Straight:
                break;

            case Steering.Right:
                float finalAngle2 = pathSegment.distance / circumference * 360f * Mathf.Deg2Rad;
                headingAgle += finalAngle2 * (pathSegment.gear == Gear.Forward ? 1 : -1);

                break;
        }

        headingAgle = WrapAngleInRadians(headingAgle);

        return headingAgle * Mathf.Rad2Deg;
    }


    public static float WrapAngleInRadians(float angle)
    {
        float PI = Mathf.PI;
        float TWO_PI = PI * 2f;

        angle = (float)System.Math.IEEERemainder((double)angle, (double)TWO_PI);

        if (angle > 2f * PI)
        {
            angle = angle - 2f * PI;
        }
        if (angle < 0f)
        {
            angle = 2f * PI + angle;
        }

        return angle;
    }
}
