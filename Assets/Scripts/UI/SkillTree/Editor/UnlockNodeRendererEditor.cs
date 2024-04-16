using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(UnlockNodeRenderer), true)]
[CanEditMultipleObjects]
public class UnlockNodeRendererEditor : GraphicEditor
{
    protected SerializedProperty LineColor;
    protected override void OnEnable()
    {
        base.OnEnable();

        LineColor = serializedObject.FindProperty(nameof(UnlockNodeRenderer.LineColor));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(LineColor);
        serializedObject.ApplyModifiedProperties();
        base.OnInspectorGUI();
    }
}