using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(GameManager))]
public class GameManagerCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GameManager gamManager = (GameManager)target;
        GUILayout.Space(10);
        if (GUILayout.Button("Play Game"))
        {
            gamManager.playGame();
        }
        if (GUILayout.Button("Restart Game"))
        {
            gamManager.restartGame();
        }
        if (GUILayout.Button("Stop Game"))
        {
            gamManager.exitGame();
        }

        GUILayout.Space(10);
        
        DrawDefaultInspector();
    }
}
#endif