using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace YNode.Editor
{
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public class TooltipDrawer<T> : OdinAttributeDrawer<TooltipAttribute, T>
    {
        protected override bool CanDrawAttributeValueProperty(InspectorProperty property) => property.GetAttribute<HideLabelAttribute>() is null;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            GUILayout.Space(0);
            var position = GUILayoutUtility.GetLastRect();
            CallNextDrawer(label);
            var iconRect = position;
            iconRect.height = EditorGUIUtility.singleLineHeight;
            iconRect.width = iconRect.height;
            if (GraphWindow.InNodeEditor)
                iconRect.x -= iconRect.width * 0.4f;
            else
                iconRect.x -= iconRect.width;

            //var icon = EditorIcons.UnityInfoIcon;
            var previousColor = GUI.color;
            GUI.color = new(1f, 1f, 1f, 0.25f);
            GUI.Label(iconRect, "?");
            GUI.color = previousColor;
        }
    }
}
