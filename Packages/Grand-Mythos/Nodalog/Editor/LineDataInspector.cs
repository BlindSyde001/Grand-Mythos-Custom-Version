#nullable enable

using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Nodalog.Editor
{
    [Inspector(typeof(LineData))]
    public class LineDataInspector : Inspector
    {
        public LineDataInspector(Metadata metadata) : base(metadata) { }

        protected override bool cacheHeight => !metadata.HasAttribute<InspectorTextAreaAttribute>();

        private float GetFieldHeight(float width, GUIContent label)
        {
            if (metadata.HasAttribute<InspectorTextAreaAttribute>())
            {
                var attribute = metadata.GetAttribute<InspectorTextAreaAttribute>();

                var height = LudiqStyles.textAreaWordWrapped.CalcHeight(new GUIContent(((LineData)metadata.value).RawString), WidthWithoutLabel(metadata, width, label));

                if (attribute.hasMinLines)
                {
                    var minHeight = EditorStyles.textArea.lineHeight * attribute.minLines + EditorStyles.textArea.padding.top + EditorStyles.textArea.padding.bottom;

                    height = Mathf.Max(height, minHeight);
                }

                if (attribute.hasMaxLines)
                {
                    var maxHeight = EditorStyles.textArea.lineHeight * attribute.maxLines + EditorStyles.textArea.padding.top + EditorStyles.textArea.padding.bottom;

                    height = Mathf.Min(height, maxHeight);
                }

                return height;
            }
            else
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        protected override float GetHeight(float width, GUIContent label)
        {
            float height = HeightWithLabel(metadata, width, GetFieldHeight(width, label), label);
            height = Mathf.Max(height, EditorGUIUtility.singleLineHeight * 2);
            return height;
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            position = BeginLabeledBlock(metadata, position, label);

            var height = Mathf.Max(GetFieldHeight(position.width, GUIContent.none), EditorGUIUtility.singleLineHeight * 2);
            var fieldPosition = position.VerticalSection(ref y, height);

            string newValue;

            if (metadata.HasAttribute<InspectorTextAreaAttribute>())
            {
                newValue = EditorGUI.TextArea(fieldPosition, ((LineData)metadata.value).RawString, EditorStyles.textArea);
            }
            else if (metadata.HasAttribute<InspectorDelayedAttribute>())
            {
                newValue = EditorGUI.DelayedTextField(fieldPosition, ((LineData)metadata.value).RawString, EditorStyles.textField);
            }
            else
            {
                newValue = EditorGUI.TextField(fieldPosition, ((LineData)metadata.value).RawString, EditorStyles.textField);
            }

            if (EndBlock(metadata))
            {
                metadata.RecordUndo();
                ((LineData)metadata.value).RawString = newValue;
            }
        }

        public override float GetAdaptiveWidth()
        {
            return Mathf.Max(130, LudiqGUI.GetTextFieldAdaptiveWidth(((LineData)metadata.value).RawString));
        }
    }
}