using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;


namespace Editor
{
    [CustomPropertyDrawer(typeof(TooltipAttribute))]
    public class TooltipDrawer : DecoratorDrawer
    {
        new TooltipAttribute attribute => (TooltipAttribute)base.attribute;
        public override bool CanCacheInspectorGUI() => false;

        public override float GetHeight()
        {
            return -2; // There is some padding included for some reason
        }

        public override void OnGUI(Rect position)
        {
            var iconRect = position;
            iconRect.height = EditorGUIUtility.singleLineHeight;
            iconRect.width = iconRect.height;
            iconRect.x += EditorGUIUtility.labelWidth - iconRect.width;

            //var icon = EditorIcons.UnityInfoIcon;
            var previousColor = GUI.color;
            GUI.color = new(1f, 1f, 1f, 0.15f);
            GUI.Label(iconRect, "?");
            GUI.color = previousColor;

            var hover = position;
            hover.height = EditorGUIUtility.singleLineHeight;
            if (hover.Contains(Event.current.mousePosition))
            {
                var icon = EditorIcons.UnityInfoIcon;
                var style = SirenixGUIStyles.MessageBox;
                var infoBoxRect = position;
                infoBoxRect.height = style.CalcHeight(GUIHelper.TempContent(attribute.Text, icon), infoBoxRect.width);
                infoBoxRect.y -= infoBoxRect.height;
                EditorGUI.DrawRect(infoBoxRect, Color.Lerp(Color.black, Color.gray, 0.5f));
                GUI.Label(infoBoxRect, GUIHelper.TempContent(attribute.Text, icon), style);
            }
        }
    }
}