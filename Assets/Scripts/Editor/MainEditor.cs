using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MainManager))]
public class MainEditor : Editor
{
    public override void OnInspectorGUI () {
        //Default
        DrawDefaultInspector();

        //New extra stuff
        if (GUILayout.Button("Map Auditor")) {
            MainManager.MapAudit();
        }
    }
}
