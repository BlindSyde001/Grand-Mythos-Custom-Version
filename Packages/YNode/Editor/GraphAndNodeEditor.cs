using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector.Editor;

namespace YNode.Editor
{
    /// <summary> Override graph inspector to show an 'Open Graph' button at the top </summary>
    [CustomEditor(typeof(NodeGraph), true)]
    public class GlobalGraphEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Edit graph", GUILayout.Height(40)))
            {
                GraphWindow.Open((NodeGraph)serializedObject.targetObject);
            }

            base.OnInspectorGUI();
        }
    }
}