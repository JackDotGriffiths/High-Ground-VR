using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NodeComponent))]
public class NodeComponentEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        NodeComponent _nodeComp = (NodeComponent)target;
        GUILayout.Space(5);
        GUILayout.Label("Node Label : " + _nodeComp.node.label);
        GUILayout.Space(3);
        GUILayout.Label("Node Navigability : " + _nodeComp.node.navigability);
        GUILayout.Space(3);
        GUILayout.Label("Node X : " + _nodeComp.node.label);
        GUILayout.Space(3);
        GUILayout.Label("Node Y : " + _nodeComp.node.label);
        GUILayout.Space(3);
        GUILayout.Label("Node Hex Position : " + _nodeComp.node.hex.transform.position);


        GUILayout.Space(20);
        if (GUILayout.Button("Place Barracks"))
        {
            _nodeComp.PlaceBarracks();
        }
        if (GUILayout.Button("Place Mine"))
        {
            _nodeComp.PlaceMine();
        }
        if (GUILayout.Button("Place Walls"))
        {
            _nodeComp.PlaceWalls();
        }
    }
}
