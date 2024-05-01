using UnityEngine;

public class Car : MonoBehaviour
{
    public Vector3 RearWheelPosition { get; set; }
    public float HeadingAngle { get; set; }

    public Car(Vector3 rearWheelPosition, float headingAngle)
    {
        RearWheelPosition = rearWheelPosition;
        HeadingAngle = headingAngle;
    }

    public void Awake()
    {
        HeadingAngle = transform.eulerAngles.y;
        RearWheelPosition = transform.position + transform.rotation * new Vector3(0f, 0f, -1.43f);
    }

    public void Update()
    {
        HeadingAngle = transform.eulerAngles.y;
        RearWheelPosition = transform.position + transform.rotation * new Vector3(0f, 0f, -1.43f);
    }

    public static Vector3 CalculatePositionAfterMovement(Vector3 rearWheelPosition, float headingAngle,
        Movement movement)
    {
        headingAngle *= Mathf.Deg2Rad;

        var newRearWheelPos = Vector3.zero;

        switch (movement.SteeringVal)
        {
            case Movement.Steering.Left:
                var cx = rearWheelPosition.x - Mathf.Cos(headingAngle) * Constants.CarTurningRadius;
                var cz = rearWheelPosition.z + Mathf.Sin(headingAngle) * Constants.CarTurningRadius;

                var finalAngle = movement.Distance / Constants.CarTurningCircumference * 360f * Mathf.Deg2Rad;

                headingAngle += finalAngle * (movement.GearVal == Movement.Gear.Forward ? -1 : 1);

                var x = cx + Constants.CarTurningRadius * Mathf.Cos(headingAngle);
                var z = cz - Constants.CarTurningRadius * Mathf.Sin(headingAngle);

                newRearWheelPos = new Vector3(x, rearWheelPosition.y, z);

                break;

            case Movement.Steering.Straight:
                newRearWheelPos.x = rearWheelPosition.x + movement.Distance * Mathf.Sin(headingAngle) *
                    (movement.GearVal == Movement.Gear.Forward ? 1 : -1);
                newRearWheelPos.z = rearWheelPosition.z + movement.Distance * Mathf.Cos(headingAngle) *
                    (movement.GearVal == Movement.Gear.Forward ? 1 : -1);

                break;

            case Movement.Steering.Right:
                var cx2 = rearWheelPosition.x + Mathf.Cos(headingAngle) * Constants.CarTurningRadius;
                var cz2 = rearWheelPosition.z - Mathf.Sin(headingAngle) * Constants.CarTurningRadius;

                var finalAngle2 = movement.Distance / Constants.CarTurningCircumference * 360f * Mathf.Deg2Rad;

                headingAngle += finalAngle2 * (movement.GearVal == Movement.Gear.Forward ? 1 : -1);

                var x2 = cx2 - Constants.CarTurningRadius * Mathf.Cos(headingAngle);
                var z2 = cz2 + Constants.CarTurningRadius * Mathf.Sin(headingAngle);

                newRearWheelPos = new Vector3(x2, rearWheelPosition.y, z2);

                break;
        }

        return newRearWheelPos;
    }

    public static float CalculateHeadingAfterMovement(float headingAngle, Movement movement)
    {
        headingAngle *= Mathf.Deg2Rad;

        switch (movement.SteeringVal)
        {
            case Movement.Steering.Left:
                var finalAngle = movement.Distance / Constants.CarTurningCircumference * 360f * Mathf.Deg2Rad;
                headingAngle += finalAngle * (movement.GearVal == Movement.Gear.Forward ? -1 : 1);
                break;
            case Movement.Steering.Straight:
                break;
            case Movement.Steering.Right:
                var finalAngle2 = movement.Distance / Constants.CarTurningCircumference * 360f * Mathf.Deg2Rad;
                headingAngle += finalAngle2 * (movement.GearVal == Movement.Gear.Forward ? 1 : -1);
                break;
        }

        headingAngle *= Mathf.Rad2Deg;

        return Utils.WrapAngle(headingAngle);
    }

    public void DrawShape()
    {
        Gizmos.color = Color.blue;

        var carRotation = transform.rotation;
        Vector3 halfExtents = new(Constants.CarWidth / 2f, Constants.CarHeight / 2f, Constants.CarLength / 2f);
        var center = transform.position + carRotation * new Vector3(0f, halfExtents.y, 0f);

        var oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(center, carRotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2);

        Gizmos.matrix = oldMatrix;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(RearWheelPosition, 0.1f);
    }
}