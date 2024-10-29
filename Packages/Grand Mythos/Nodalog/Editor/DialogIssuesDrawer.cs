#nullable enable

using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Nodalog.Editor
{
    [Inspector(typeof(DialogIssues))]
    public class DialogIssuesDrawer : Inspector
    {
        static Texture? _dummyTexture;

        public DialogIssuesDrawer(Metadata metadata) : base(metadata) { }

        protected override float GetHeight(float width, GUIContent label)
        {
            _dummyTexture ??= EditorGUIUtility.ObjectContent(null, typeof(MonoBehaviour))?.image;
            var v = (DialogIssues)metadata.value;
            float total = 0f;
            foreach (var message in v.Issues)
                total += EditorStyles.helpBox.CalcHeight(new(message.Text, _dummyTexture), width);
            return total;
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            _dummyTexture ??= EditorGUIUtility.ObjectContent(null, typeof(MonoBehaviour))?.image;
            var issues = (DialogIssues)metadata.value;
            foreach (var message in issues.Issues)
            {
                var height = EditorStyles.helpBox.CalcHeight(new(message.Text, _dummyTexture), position.width);
                position.height = height;
                EditorGUI.HelpBox(position, message.Text, (MessageType)message.Type);
                position.y += position.height;
            }
        }

        public override float GetAdaptiveWidth()
        {
            float maxWidth = base.GetAdaptiveWidth();
            foreach (var issues in ((DialogIssues)metadata.value).Issues)
                maxWidth = Mathf.Max(LudiqGUI.GetTextFieldAdaptiveWidth(issues.Text), maxWidth);

            return maxWidth;
        }
    }
}