using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace YNode.Editor
{
    [Serializable]
    public class NodeEditor : ScriptableObject
    {
        [SkipPolymorphicField, SerializeReference, HideLabel, InlineProperty, ShowInInspector]
        public INodeValue Value = null!;

        [NonSerialized] public SerializedObject SerializedObject = null!;
        [NonSerialized] public PropertyTree ObjectTree = null!;
        [NonSerialized] public GraphWindow Window = null!;
        public Dictionary<string, List<Vector2>> ReroutePoints = new();

        private Dictionary<string, Port> _ports = new();

        public Vector2 CachedSize { get; set; }
        private string? _title;
        public NodeGraph Graph => Window.Graph;

        /// <summary> Iterate over all ports on this node. </summary>
        internal Dictionary<string, Port> Ports => _ports;

        public bool IsSelected() => Selection.Contains(this);

        /// <summary> Add a dynamic, serialized port to this node. </summary>
        public Port AddPort(string fieldName, Type type, IO direction, GetConnected getConnected,
            CanConnectTo canConnectTo, SetConnection setConnection, NoodleStroke stroke, string? tooltip = null)
        {
            if (HasPort(fieldName))
            {
                Debug.LogWarning($"Port '{fieldName}' already exists in {name}", this);
                return _ports[fieldName];
            }

            Port port = new Port(fieldName, this, type, direction, getConnected, canConnectTo, setConnection, stroke, tooltip);
            _ports.Add(fieldName, port);
            return port;
        }


        /// <summary> Remove a dynamic port from the node </summary>
        public void RemovePort(string fieldName, bool disconnect)
        {
            Port? dynamicPort = GetPort(fieldName);
            if (dynamicPort == null) throw new ArgumentException($"port {fieldName} doesn't exist");
            RemovePort(dynamicPort, disconnect);
        }

        /// <summary> Remove a dynamic port from the node </summary>
        public void RemovePort(Port port, bool disconnect)
        {
            if (disconnect)
                port.Disconnect();
            _ports.Remove(port.FieldName);
        }

        /// <summary> Returns port which matches fieldName </summary>
        public Port? GetPort(string fieldName)
        {
            return _ports.GetValueOrDefault(fieldName);
        }

        public bool HasPort(string fieldName)
        {
            return _ports.ContainsKey(fieldName);
        }

        /// <summary> Disconnect everything from this node </summary>
        public void ClearConnections()
        {
            foreach ((_, Port port) in _ports)
                port.Disconnect();
        }

        public virtual void OnHeaderGUI()
        {
            _title ??= ObjectNames.NicifyVariableName(Value.GetType().Name);
            GUILayout.Label(_title, Resources.Styles.NodeHeader, GUILayout.Height(30));
        }

        /// <summary> Draws standard field editors for all public fields </summary>
        public virtual void OnBodyGUI()
        {
            GraphWindow.InNodeEditor = true;

            // Unity specifically requires this to save/update any serial object.
            // serializedObject.Update(); must go at the start of an inspector gui, and
            // serializedObject.ApplyModifiedProperties(); goes at the end.
            // Although it doesn't seem like we need it because of how NodeEditors are just referencing graph nodes instead of hosting them
            // SerializedObject.Update();

            try
            {
                ObjectTree.BeginDraw(false);
            }
            catch (ArgumentNullException)
            {
                ObjectTree.EndDraw();
                return;
            }

            GUIHelper.PushLabelWidth(84);
            EditorGUI.BeginChangeCheck();
            ObjectTree.DrawProperties();
            ObjectTree.EndDraw();
            GUIHelper.PopLabelWidth();

            //SerializedObject.ApplyModifiedProperties();

            // Call repaint so that the graph window elements respond properly to layout changes coming from Odin
            if (GUIHelper.RepaintRequested)
            {
                GUIHelper.ClearRepaintRequest();
                Window.Repaint();
            }
            GraphWindow.InNodeEditor = false;
        }

        public virtual int GetWidth()
        {
            Type type = Value.GetType();
            return type.TryGetAttributeWidth(out var width) ? width : NodeWidthAttribute.Default;
        }

        /// <summary> Returns color for target node </summary>
        public virtual Color GetTint()
        {
            Type type = Value.GetType();
            return Window.GetTypeColor(type);
        }

        public virtual GUIStyle GetBodyStyle()
        {
            return Resources.Styles.NodeBody;
        }

        public virtual GUIStyle GetBodyHighlightStyle()
        {
            return Resources.Styles.NodeHighlight;
        }

        /// <summary> Override to display custom node header tooltips </summary>
        public virtual string? GetHeaderTooltip()
        {
            return null;
        }

        /// <summary> Add items for the context menu when right-clicking this node. Override to add custom menu items. </summary>
        public virtual void AddContextMenuItems(GenericMenu menu)
        {
            bool canRemove = true;
            // Actions if only one node is selected
            if (Selection.objects.Length == 1 && Selection.activeObject is NodeEditor node)
            {
                menu.AddItem(new GUIContent("Move To Top"), false, () => Window.MoveNodeToTop(node));

                canRemove = Window.CanRemove(node);
            }

            // Add actions to any number of selected nodes
            menu.AddItem(new GUIContent("Copy"), false, Window.CopySelectedNodes);
            menu.AddItem(new GUIContent("Duplicate"), false, Window.DuplicateSelectedNodes);
            menu.AddItem(new GUIContent("Remove"), false, canRemove ? Window.RemoveSelectedNodes : null);

            // Custom sctions if only one node is selected
            if (Selection.objects.Length == 1 && Selection.activeObject is NodeEditor node2)
            {
                menu.AddCustomContextMenuItems(node2);
            }
        }
    }
}
