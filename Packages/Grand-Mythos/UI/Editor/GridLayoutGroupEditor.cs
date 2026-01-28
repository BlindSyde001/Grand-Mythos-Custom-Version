using UnityEditor;


/// <summary>
/// Custom Editor for the GridLayout Component.
/// Extend this class to write a custom editor for a component derived from GridLayout.
/// </summary>
[CustomEditor(typeof(SlantedGridLayoutGroup), true)]
[CanEditMultipleObjects]
public class GridLayoutGroupEditor : UnityEditor.Editor
{
    SerializedProperty m_Padding = null!;
    SerializedProperty m_CellSize = null!;
    SerializedProperty m_Spacing = null!;
    SerializedProperty m_StartCorner = null!;
    SerializedProperty m_StartAxis = null!;
    SerializedProperty m_ChildAlignment = null!;
    SerializedProperty m_Constraint = null!;
    SerializedProperty m_ConstraintCount = null!;
    SerializedProperty m_Slantedness = null!;

    protected virtual void OnEnable()
    {
        m_Padding = serializedObject.FindProperty("m_Padding");
        m_CellSize = serializedObject.FindProperty("m_CellSize");
        m_Spacing = serializedObject.FindProperty("m_Spacing");
        m_StartCorner = serializedObject.FindProperty("m_StartCorner");
        m_StartAxis = serializedObject.FindProperty("m_StartAxis");
        m_ChildAlignment = serializedObject.FindProperty("m_ChildAlignment");
        m_Constraint = serializedObject.FindProperty("m_Constraint");
        m_ConstraintCount = serializedObject.FindProperty("m_ConstraintCount");
        m_Slantedness = serializedObject.FindProperty("m_Slantedness");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_Padding, true);
        EditorGUILayout.PropertyField(m_CellSize, true);
        EditorGUILayout.PropertyField(m_Spacing, true);
        EditorGUILayout.PropertyField(m_StartCorner, true);
        EditorGUILayout.PropertyField(m_StartAxis, true);
        EditorGUILayout.PropertyField(m_ChildAlignment, true);
        EditorGUILayout.PropertyField(m_Constraint, true);
        if (m_Constraint.enumValueIndex > 0)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_ConstraintCount, true);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.PropertyField(m_Slantedness, true);
        serializedObject.ApplyModifiedProperties();
    }
}

