using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InputManager))]
public class InputManagementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        InputManager inpManager = (InputManager)target;
        GUILayout.Space(10);
        if (GUILayout.Button("Set Height of Game Board"))
        {
            inpManager.updateWorldHeight();
        }
    }
}
