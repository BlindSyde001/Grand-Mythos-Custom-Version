using System;
using System.Collections.Generic;
using UnityEngine;
using YNode.Editor;
using Screenplay.Nodes;
using Sirenix.Utilities;
using UnityEditor;

namespace Screenplay.Editor
{
    public class NotesEditor : CustomNodeEditor<Notes>
    {
        private static Dictionary<int, GUIStyle> s_bodyStyleBackingField = new();

        private GUIStyle BodyStyle
        {
            get
            {
                if (s_bodyStyleBackingField.TryGetValue(Value.Size, out var style) == false)
                {
                    style = new GUIStyle(YNode.Editor.Resources.Styles.NodeBody) { normal = { background = Texture2D.whiteTexture } };
                    style.padding.top = style.padding.bottom;
                    style.active.background = Texture2D.whiteTexture;
                    style.normal.textColor = Color.white;
                    style.active.textColor = Color.white;
                    style.fontSize = 5 * Value.Size;
                }

                return style;
            }
        }


        public override void OnHeaderGUI() { }

        public override void OnBodyGUI()
        {
            var previousBG = GUI.backgroundColor;
            var previousContent = GUI.color;
            var backgroundTint = GetTint();
            GUI.backgroundColor = new Color();
            GUI.color = backgroundTint.grayscale > 0.5f ? Color.black : Color.white;
            BodyStyle.active.textColor = backgroundTint.grayscale > 0.5f ? Color.black : Color.white;
            Value.Content = EditorGUILayout.TextArea(Value.Content, BodyStyle);

            GUI.backgroundColor = previousBG;
            GUI.color = previousContent;

            if (IsSelected())
            {
                var rect = GUILayoutUtility.GetLastRect();
                rect.y += rect.height;
                rect.height = EditorGUIUtility.singleLineHeight;
                rect.width = 64;
                Value.Color = EditorGUI.ColorField(rect, Value.Color);
                rect.x += rect.width;
                if (GUI.Button(rect.AlignLeft(rect.width / 2f), "+"))
                    Value.Size *= 2;
                if (GUI.Button(rect.AlignRight(rect.width / 2f), "-"))
                    Value.Size /= 2;
            }
        }

        public override GUIStyle GetBodyStyle()
        {
            return BodyStyle;
        }

        public override GUIStyle GetBodyHighlightStyle()
        {
            return BodyStyle;
        }

        public override Color GetTint()
        {
            return Value.Color;
        }

        public override int GetWidth()
        {
            var baseWidth = base.GetWidth();
            var width = BodyStyle.CalcSize(new GUIContent(Value.Content)).x;
            width += BodyStyle.padding.left + BodyStyle.padding.right;
            return Math.Max(baseWidth, Mathf.CeilToInt(width));
        }
    }
}
