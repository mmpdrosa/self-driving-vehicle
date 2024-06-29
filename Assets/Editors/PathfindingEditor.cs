using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HybridAStarScene))]
public class PathfindingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var pathfinding = (HybridAStarScene)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Find Path."))
        {
            pathfinding.FindPath();
        }

        if (GUILayout.Button("Recalculate Costs."))
        {
            pathfinding.RecalculateCosts();
        }
    }
}