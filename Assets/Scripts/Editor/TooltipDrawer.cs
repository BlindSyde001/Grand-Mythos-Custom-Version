using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [DrawerPriority(DrawerPriorityLevel.ValuePriority)]
    public sealed class PropertyTooltipAttributeDrawer : OdinAttributeDrawer<PropertyTooltipAttribute>
    {
        ValueResolver<string> tooltipResolver;

        protected override void Initialize() => tooltipResolver = ValueResolver.GetForString(Property, Attribute.Tooltip);

        protected override bool CanDrawAttributeProperty(InspectorProperty property) => property.Info.PropertyType != PropertyType.Method;

        /// <summary>Draws the property.</summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (label != null)
            {
                if (tooltipResolver.HasError)
                    SirenixEditorGUI.ErrorMessageBox(tooltipResolver.ErrorMessage);
                label.tooltip = tooltipResolver.GetValue();
            }
            var r = GUILayoutUtility.GetLastRect();
            CallNextDrawer(label);
            r.y += EditorGUIUtility.singleLineHeight - 2;
            r.height = 1;
            r.width = 10;
            r.x += 2;
            EditorGUI.DrawRect(r, new Color(1, 1, 1, 0.5f));
        }
    }
}