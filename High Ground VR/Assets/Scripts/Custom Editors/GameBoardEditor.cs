using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameBoardGeneration))]
public class GameBoardEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GameBoardGeneration genTerrain = (GameBoardGeneration)target;
        GUILayout.Space(10);
        if (GUILayout.Button("Gen Terrain"))
        {
            genTerrain.generate();
        }
        GUILayout.Space(3);
        if (GUILayout.Button("Clear Nature"))
        {
            genTerrain.destroyAmbientNature();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Clear Terrain"))
        {
            genTerrain.destroyAll();
        }

    }
}
