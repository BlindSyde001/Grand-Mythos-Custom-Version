using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FieldOfViewEditor : MonoBehaviour
{

    [CustomEditor(typeof(CharacterMovement))]
    public class FieldOfView : Editor
    {
        private void OnSceneGUI()
        {
            CharacterMovement fov = (CharacterMovement)target;
            Handles.color = Color.red;
            Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.minRadius);
            Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.maxRadius);
        }
    }
}
