using UnityEngine;

public static class Constants
{
    public const float CellSize = 0.5f;

    public const float HeadingAngleStep = 15f;

    public const float CarTurningRadius = 5.9f;
    public const float CarTurningCircumference = 2 * Mathf.PI * CarTurningRadius;
    public const float CarLength = 5.021f;
    public const float CarWidth = 2.189f;
    public const float CarHeight = 1.431f;
    public const float CarWheelBase = 2.96f;


    public const float DriveDistance = 0.8f;

    public const float ChangingDirectionCost = 0.5f;
    public const float ReversingCost = 1.5f;
    public const float TurningCost = 0f;

    public const int Iterations = 100_000;

    public const float GoalDistanceThreshold = 0.5f;
    public const float GoalHeadingAngleThreshold = 15f;


    public const float VoronoiAlpha = 10f;
    public const float VoronoiMaxObstacleDistance = 50f;
}