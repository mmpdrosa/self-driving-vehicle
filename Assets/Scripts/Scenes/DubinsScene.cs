using UnityEngine;
using Path = System.Collections.Generic.List<Movement>;

public class DubinsScene : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;

    [SerializeField] private Transform _startCar, _goalCar;

    void OnDrawGizmos()
    {
        if (_startCar == null || _goalCar == null) return;

        var startPosition = _startCar.transform.position;
        var startRotation = _startCar.transform.rotation;

        var goalPosition = _goalCar.transform.position;
        var goalRotation = _goalCar.transform.rotation;

        var path = ReedsShepp.GetOptimalPath(
            startPosition.x,
            startPosition.z,
            startRotation.eulerAngles.y,
            goalPosition.x,
            goalPosition.z,
            goalRotation.eulerAngles.y
        );

        DisplayPath(path);
    }

    private void DisplayPath(Path path)
    {
        var waypoints =
            PathBuilder.GenerateWaypoints(_startCar.transform.position, _startCar.transform.rotation.eulerAngles.y,
                path, 10);

        lineRenderer.positionCount = waypoints.Count;
        lineRenderer.SetPositions(waypoints.ToArray());
    }
}