using System.Linq;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(UnlockNodeRenderer), true)]
    [CanEditMultipleObjects]
    public class UnlockNodeRendererEditor : ImageEditor
    {
        UnlockNode? _node;
        protected SerializedProperty LineColor = null!;

        protected override void OnEnable()
        {
            base.OnEnable();

            LineColor = serializedObject.FindProperty(nameof(UnlockNodeRenderer.LineColor));
            _node = (target as UnlockNodeRenderer)?.gameObject.GetComponent<UnlockNode>();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(LineColor);
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

        void OnSceneGUI()
        {
            var skillTree = _node?.GetComponentInParent<SkillTree>();
            if (_node is null || skillTree == null)
                return;

            for (var e = skillTree.NodesEnum(); e.MoveNext(); )
            {
                var otherNode = e.Current.Key;
                if (ReferenceEquals(otherNode, _node))
                    continue;

                var pos = otherNode.transform.position;
                float size = HandleUtility.GetHandleSize(pos) * 0.2f;

                var contains = _node.LinkedTo.Contains(otherNode);
                Handles.color = contains ? new Color(0.75f, 0.25f, 0.25f, 1) : new Color(0.25f, 0.75f, 0.25f, 1);
                if (Handles.Button(pos + Vector3.down * ((RectTransform)otherNode.transform).rect.y * 0.5f, otherNode.transform.rotation, size, size, Handles.CylinderHandleCap))
                {
                    if (contains)
                    {
                        _node.LinkedTo = _node.LinkedTo.Where(x => ReferenceEquals(x, otherNode) == false).ToArray();
                        otherNode.LinkedTo = otherNode.LinkedTo.Where(x => ReferenceEquals(x, _node) == false).ToArray();
                    }
                    else
                    {
                        _node.LinkedTo = _node.LinkedTo.Append(otherNode).ToArray();
                        otherNode.LinkedTo = otherNode.LinkedTo.Append(_node).ToArray();
                    }
                    HandleUtility.Repaint();
                }
            }

            if (Event.current.type == EventType.MouseMove)
                HandleUtility.Repaint();
        }
    }
}