using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace YNode.Editor
{
    // Must be above one of the polymorphic drawer having special handling for fields pointing to the same target
    [DrawerPriority(90, 0, 0)]
    public sealed class NodeLinkDrawer<T> : OdinValueDrawer<T>, IDisposable where T : class // Must use 'class' instead of 'INodeValue' because that test is on the field's value type not the field's type, meaning that nulls do not get decorated by this
    {
        protected override bool CanDrawValueProperty(InspectorProperty property)
        {
            return property.Tree.WeakTargets[0] is NodeEditor && typeof(INodeValue).IsAssignableFrom(property.Info.TypeOfValue) && property.GetAttribute<IOAttribute>() is not null;
        }

        protected override void Initialize()
        {
            base.Initialize();
            var node = (NodeEditor)Property.Tree.WeakTargets[0];
            if (node.GetPort(Property.UnityPropertyPath) != null)
                return;

            var valueType = Property.Info.TypeOfValue;
            string tooltip = Property.GetAttribute<TooltipAttribute>()?.tooltip ?? valueType.Name;
            var attrib = Property.Attributes.GetAttribute<IOAttribute>();
            var io = attrib is OutputAttribute ? IO.Output : IO.Input;
            node.AddPort(Property.UnityPropertyPath, valueType, io, GetConnected, CanConnectTo, SetConnection, attrib.Stroke, tooltip);

            void SetConnection(INodeValue? node1) => ValueEntry.WeakSmartValue = node1!;
            INodeValue? GetConnected() => (INodeValue?)ValueEntry.WeakSmartValue;
            bool CanConnectTo(Type type) => valueType.IsAssignableFrom(type);
        }

        public void Dispose()
        {
            if (GraphWindow.InNodeEditor) // We only care about dispose caused by changes in properties, other kinds should be handled by the graph editor
            {
                var node = (NodeEditor)Property.Tree.WeakTargets[0];
                var port = node.GetPort(Property.UnityPropertyPath);
                if (port is not null)
                    node.RemovePort(port, false);
            }
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var node = (NodeEditor)Property.Tree.WeakTargets[0];
            Port port = node.GetPort(Property.UnityPropertyPath)!;

            if (!GraphWindow.InNodeEditor)
            {
                CallNextDrawer(label);
                return;
            }

            if (Property.Tree.WeakTargets.Count > 1)
            {
                SirenixEditorGUI.WarningMessageBox("Cannot draw ports with multiple nodes selected");
                return;
            }

            LabelWidthAttribute? labelWidth = Property.GetAttribute<LabelWidthAttribute>();
            if (labelWidth != null)
                GUIHelper.PushLabelWidth(labelWidth.Width);

            PropertyField(label, port);

            if (labelWidth != null)
                GUIHelper.PopLabelWidth();
        }

        /// <summary> Make a field for a serialized property. Manual node port override. </summary>
        private void PropertyField(GUIContent? label, Port port)
        {
            if (Property.GetAttribute<RequiredAttribute>() is not null && ValueEntry.SmartValue == null)
            {
                SirenixEditorGUI.ErrorMessageBox($"{Property.NiceName} is required");
            }

            Rect rect = new();
            // If property is an input, display a regular property field and put a port handle on the left side
            if (port.Direction == IO.Input)
            {
                // Get data from [Input] attribute
                bool usePropertyAttributes = true;

                float spacePadding = 0;
                foreach (Attribute? attr in Property.Attributes)
                {
                    if (attr is SpaceAttribute spaceAttribute)
                    {
                        if (usePropertyAttributes)
                            GUILayout.Space(spaceAttribute.height);
                        else
                            spacePadding += spaceAttribute.height;
                    }
                    else if (attr is HeaderAttribute headerAttribute)
                    {
                        if (usePropertyAttributes)
                        {
                            //GUI Values are from https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/ScriptAttributeGUI/Implementations/DecoratorDrawers.cs
                            Rect position = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight * 1.5f - EditorGUIUtility.standardVerticalSpacing); //Layout adds standardVerticalSpacing after rect so we subtract it.
                            position.yMin += EditorGUIUtility.singleLineHeight * 0.5f;
                            position = EditorGUI.IndentedRect(position);
                            GUI.Label(position, headerAttribute.header, EditorStyles.boldLabel);
                        }
                        else
                            spacePadding += EditorGUIUtility.singleLineHeight * 1.5f;
                    }
                }

                if (Property.GetAttribute<HideLabelAttribute>() is not null || label == null)
                    EditorGUILayout.Space(0f);
                else
                    EditorGUILayout.LabelField(label);

                rect = GUILayoutUtility.GetLastRect();
                float paddingLeft = GraphWindow.Current.GetPortStyle(port).padding.left;
                rect.position = rect.position - new Vector2(16 + paddingLeft, -spacePadding);
                // If property is an output, display a text label and put a port handle on the right side
            }
            else if (port.Direction == IO.Output)
            {
                // Get data from [Output] attribute

                bool usePropertyAttributes = true;

                float spacePadding = 0;
                foreach (Attribute? attr in Property.Attributes)
                {
                    if (attr is SpaceAttribute spaceAttribute)
                    {
                        if (usePropertyAttributes)
                            GUILayout.Space(spaceAttribute.height);
                        else
                            spacePadding += spaceAttribute.height;
                    }
                    else if (attr is HeaderAttribute headerAttribute)
                    {
                        if (usePropertyAttributes)
                        {
                            //GUI Values are from https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/ScriptAttributeGUI/Implementations/DecoratorDrawers.cs
                            Rect position = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight * 1.5f - EditorGUIUtility.standardVerticalSpacing); //Layout adds standardVerticalSpacing after rect so we subtract it.
                            position.yMin += EditorGUIUtility.singleLineHeight * 0.5f;
                            position = EditorGUI.IndentedRect(position);
                            GUI.Label(position, headerAttribute.header, EditorStyles.boldLabel);
                        }
                        else
                            spacePadding += EditorGUIUtility.singleLineHeight * 1.5f;
                    }
                }

                if (Property.GetAttribute<HideLabelAttribute>() is not null || label == null)
                    EditorGUILayout.Space(0f);
                else
                    EditorGUILayout.LabelField(label, Resources.OutputPort, GUILayout.MinWidth(30));

                rect = GUILayoutUtility.GetLastRect();
                rect.width += GraphWindow.Current.GetPortStyle(port).padding.right;
                rect.position = rect.position + new Vector2(rect.width, spacePadding);
            }

            rect.size = new(16, 16);

            // Register the handle position
            port.CachedHeight = rect.center.y;
        }
    }
}
