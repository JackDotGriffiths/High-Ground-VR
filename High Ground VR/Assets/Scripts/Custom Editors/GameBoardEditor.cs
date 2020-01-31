using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameBoardManagement))]
public class GameBoardEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameBoardManagement genTerrain = (GameBoardManagement)target;
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
