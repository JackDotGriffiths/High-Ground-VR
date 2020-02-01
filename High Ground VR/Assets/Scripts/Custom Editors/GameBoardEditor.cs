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
        if(GUILayout.Button("Gen Terrain"))
        {
            genTerrain.generate();
        }
        if (GUILayout.Button("Clear Terrain"))
        {
            genTerrain.destroyAll();
        }

    }
}
