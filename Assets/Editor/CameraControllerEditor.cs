using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(CameraController))]
public class CameraControllerEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CameraController cf = (CameraController)target;
    }
}
