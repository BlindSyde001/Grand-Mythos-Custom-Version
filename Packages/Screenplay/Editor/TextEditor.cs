using System.Reflection;
using Screenplay;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Source.Screenplay.Editor
{
    public class TextEditor : OdinAttributeDrawer<TextAttribute>
    {
        private static GUIContent s_content = new();
        private static FieldInfo? s_lastControl;
        private static GUIStyle? s_richTextStyle;

        private Vector2 _cachedSize;
        private bool _richText;
        private bool _focused, _keepFocused;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            string str = (string)Property.ValueEntry.WeakSmartValue;
            s_content.text = str;

            GUIStyle style;
            if (_richText)
            {
                s_richTextStyle ??= new GUIStyle(GUI.skin.textArea) { richText = true };
                style = s_richTextStyle;
            }
            else
            {
                style = GUI.skin.textArea;
            }

            Rect rect = GUILayoutUtility.GetRect(s_content, style);
            rect.height += GUILayoutUtility.GetRect(rect.width, EditorGUIUtility.singleLineHeight).height;

            Property.ValueEntry.WeakSmartValue = EditorGUI.TextArea(rect, s_content.text, style);

            if (Event.current.type == EventType.Repaint)
                _focused = GUIUtility.keyboardControl == GetLastId();

            var richTextButton = rect;
            richTextButton.width = 100;
            richTextButton.height = EditorGUIUtility.singleLineHeight;
            richTextButton.y -= richTextButton.height;
            if (Event.current.type == EventType.Repaint)
            {
                if (richTextButton.Contains(Event.current.mousePosition))
                {
                    if (_focused)
                        _keepFocused = true;
                }
                else
                {
                    _keepFocused = false;
                }
            }

            if (_focused || _keepFocused)
            {
                if (GUI.Button(richTextButton, _richText ? "Raw" : "Rich"))
                {
                    _richText = !_richText;
                }
            }
        }

        static int GetLastId()
        {
            s_lastControl ??= typeof( EditorGUIUtility ).GetField( "s_LastControlID", BindingFlags.Static | BindingFlags.NonPublic );
            return (int)s_lastControl!.GetValue( null );
        }
    }
}
