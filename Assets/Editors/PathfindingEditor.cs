using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Pathfinding))]
public class PathfindingEditor : Editor
{

    public override void OnInspectorGUI()
    {
        Pathfinding pathfinding = (Pathfinding)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Clique para fazer algo"))
        {
            pathfinding.Find();
        }
    }   
}
