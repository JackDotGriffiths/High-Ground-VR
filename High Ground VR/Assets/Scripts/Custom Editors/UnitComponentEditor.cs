using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UnitComponent))]
public class UnitComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        try
        {
            UnitComponent _UnitComp = (UnitComponent)target;
            GUILayout.Space(30);
            GUILayout.Label("Unit Health : " + _UnitComp.unit.health);
            GUILayout.Space(3);
            GUILayout.Label("Unit Damage : " + _UnitComp.unit.damage);
            GUILayout.Space(3);
            GUILayout.Label("Unit Aggression (1.0: Angry 0.0: Passive) : " + _UnitComp.unit.aggression);
            GUILayout.Space(3);
        }
        catch
        {

        }
    }
}
