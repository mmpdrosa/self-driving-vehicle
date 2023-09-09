using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

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


    public static Vector3 CalculatePositionAfterMovement(Vector3 rearWheelPosition, float headingAgle, float driveDistance, float steeringAngle)
    {
        headingAgle *= Mathf.Deg2Rad;
        steeringAngle *= Mathf.Deg2Rad;

        Vector3 newRearWheelPos = Vector3.zero;

        float turningAngle = (driveDistance / Car.wheelBase) * Mathf.Tan(steeringAngle);

        if (Mathf.Abs(turningAngle) < 0.00001f)
        {
            newRearWheelPos.x = rearWheelPosition.x + driveDistance * Mathf.Sin(headingAgle);
            newRearWheelPos.z = rearWheelPosition.z + driveDistance * Mathf.Cos(headingAgle);
        }
        else
        {
            // float R = driveDistance / turningAngle;
            float R = turningRadius;

            float cx = rearWheelPosition.x + Mathf.Cos(headingAgle) * R;
            float cz = rearWheelPosition.z - Mathf.Sin(headingAgle) * R;

            newRearWheelPos.x = cx - Mathf.Cos(headingAgle + turningAngle) * R;
            newRearWheelPos.z = cz + Mathf.Sin(headingAgle + turningAngle) * R;
        }

        return newRearWheelPos;
    }

    public static float CalculateHeadingAngleAfterMovement(float headingAgle, float driveDistance, float steeringAngle)
    {
        float turningAngle = (driveDistance / Car.wheelBase) * Mathf.Tan(steeringAngle * Mathf.Deg2Rad);

        headingAgle = headingAgle * Mathf.Deg2Rad + turningAngle;

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
