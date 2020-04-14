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
        GameManager gameManager = (GameManager)target;
        GUILayout.Space(10);
        if (GUILayout.Button("Play Game"))
        {
            gameManager.playGame();
        }
        if (GUILayout.Button("Restart Game"))
        {
            gameManager.restartGame();
        }
        if (GUILayout.Button("Stop Game"))
        {
            gameManager.exitGame();
        }
        if (GUILayout.Button("Go To Main Menu"))
        {
            gameManager.GoToMainMenu();
        }

        GUILayout.Space(10);
        
        DrawDefaultInspector();
    }
}
#endif