using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenManagement))]
public class TerrainGenEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TerrainGenManagement genTerrain = (TerrainGenManagement)target;
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
