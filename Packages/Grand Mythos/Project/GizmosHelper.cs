using UnityEngine;

public static class GizmosHelper
{
    public static void Label(Vector3 position, string label, Color? color = null)
    {
        #if UNITY_EDITOR
        var previousColor = GUI.color;
        GUI.color = color ?? previousColor;
        UnityEditor.Handles.Label(position, label);
        GUI.color = previousColor;
        #endif
    }
}