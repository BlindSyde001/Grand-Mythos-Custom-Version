using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER && USE_ADVANCED_GENERIC_MENU
using GenericMenu = XNodeEditor.AdvancedGenericMenu;
#endif

namespace YNode.Editor
{
    /// <summary> Base class to derive custom Node Graph editors from. Use this to override how graphs are drawn in the editor. </summary>
    public partial class GraphWindow
    {
        private static Dictionary<(Color, Color), Gradient> s_gradientCache = new();

        public static bool InNodeEditor = false;

        private Dictionary<INodeValue, NodeEditor> _nodesToEditor = new();

        public static GraphWindow Current { get; private set; } = null!;

        public IReadOnlyDictionary<INodeValue, NodeEditor> NodesToEditor => _nodesToEditor;

        [NonSerialized] private bool _ranLoad;

        protected virtual void Load()
        {
            // OnEnable runs too soon, the window creation function doesn't have time to provide the data necessary.
            // and CreateGUI runs after OnGUI for some reason ...
            _ranLoad = true;
            Current = this;
            foreach (INodeValue nodeValue in Graph.Nodes)
            {
                InitNodeEditorFor(nodeValue);
            }
        }

        protected virtual void OnEnable() { }

        protected virtual void OnDisable()
        {
            Current = this;
            foreach (var editor in _nodesToEditor)
            {
                editor.Value.SerializedObject.Dispose();
                editor.Value.ObjectTree.Dispose();
                DestroyImmediate(editor.Value);
            }
        }

        public virtual void OnFocus() { }

        public virtual void OnLostFocus() { }

        public virtual Texture2D GetGridTexture()
        {
            return Preferences.GetSettings().GridTexture;
        }

        public virtual Texture2D GetSecondaryGridTexture()
        {
            return Preferences.GetSettings().CrossTexture;
        }

        /// <summary> Return default settings for this graph type. This is the settings the user will load if no previous settings have been saved. </summary>
        public virtual Preferences.Settings GetDefaultPreferences()
        {
            return new Preferences.Settings();
        }

        /// <summary> Returns context node menu path. Null or empty strings for hidden nodes. </summary>
        public virtual string GetNodeMenuName(Type type)
        {
            //Check if type has the CreateNodeMenuAttribute
            if (Utilities.GetAttrib<CreateNodeMenuAttribute>(type, out var attrib)) // Return custom path
                return attrib.menuName;
            else // Return generated path
                return Utilities.NodeDefaultPath(type);
        }

        /// <summary> The order by which the menu items are displayed. </summary>
        public virtual int GetNodeMenuOrder(Type type)
        {
            //Check if type has the CreateNodeMenuAttribute
            if (Utilities.GetAttrib<CreateNodeMenuAttribute>(type, out var attrib)) // Return custom path
                return attrib.order;
            else
                return 0;
        }

        /// <summary>
        /// Add items for the context menu when right-clicking this node.
        /// Override to add custom menu items.
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="canConnectTo">Use it to filter only nodes compatible with this port</param>
        public virtual void AddContextMenuItems(GenericMenu menu, CanConnectTo? canConnectTo, Action<NodeEditor>? onNewNode)
        {
            Vector2 pos = WindowToGridPosition(Event.current.mousePosition);

            Type[] nodeTypes;

            if (canConnectTo != null && Preferences.GetSettings().CreateFilter)
            {
                nodeTypes = GetCompatibleNodesTypes(canConnectTo).OrderBy(GetNodeMenuOrder).ToArray();

                static IEnumerable<Type> GetCompatibleNodesTypes(CanConnectTo canConnectTo)
                {
                    foreach (Type findType in NodeEditorReflection.NodeTypes)
                    {
                        if (canConnectTo(findType))
                        {
                            yield return findType;
                        }
                    }
                }
            }
            else
            {
                nodeTypes = NodeEditorReflection.NodeTypes.OrderBy(GetNodeMenuOrder).ToArray();
            }

            var menuItems = new List<(string path, Type type)>();
            for (int i = 0; i < nodeTypes.Length; i++)
            {
                Type type = nodeTypes[i];

                //Get node context menu path
                string path = GetNodeMenuName(type);
                if (string.IsNullOrEmpty(path))
                    continue;

                menuItems.Add((path, type));
            }

            foreach (var (path, type) in menuItems)
            {
                // Check if user is allowed to add more of given node type
                bool disallowed = false;
                if (Utilities.GetAttrib<DisallowMultipleNodesAttribute>(type, out var disallowAttrib))
                {
                    int typeCount = Graph.Nodes.Count(x => x.GetType() == type);
                    disallowed = typeCount >= disallowAttrib.max;
                }

                var splitPath = path.Split('/').Select(x => (path:x, merge:true)).ToArray();
                foreach (var menuItem in menuItems)
                {
                    if (ReferenceEquals(menuItem.path, path))
                        continue;

                    var otherSplitPath = menuItem.path.Split('/');
                    for (int i = 0; i < otherSplitPath.Length-1 && i < splitPath.Length-1; i++)
                    {
                        if (otherSplitPath[i] == splitPath[i].path && otherSplitPath[i+1] != splitPath[i+1].path)
                        {
                            splitPath[i].merge = false;
                            break;
                        }
                    }
                }

                var newPath = string.Concat(splitPath.Select(x => x.merge ? $"{x.path}." : $"{x.path}/"))[..^1];

                // Add node entry to context menu
                if (disallowed)
                    menu.AddItem(new GUIContent(newPath), false, null);
                else
                    menu.AddItem(new GUIContent(newPath), false, () =>
                    {
                        var node = CreateNode(type, pos);
                        onNewNode?.Invoke(node);
                    });
            }

            menu.AddSeparator("");
            if (CopyBuffer.Length > 0)
                menu.AddItem(new GUIContent("Paste"), false, () => PasteNodes(pos));
            else menu.AddDisabledItem(new GUIContent("Paste"));
            menu.AddItem(new GUIContent("Preferences"), false, NodeEditorReflection.OpenPreferences);
            menu.AddCustomContextMenuItems(Graph);
        }

        /// <summary> Returned gradient is used to color noodles </summary>
        /// <param name="output"> The output this noodle comes from. Never null. </param>
        /// <param name="input"> The output this noodle comes from. Can be null if we are dragging the noodle. </param>
        public virtual Gradient GetNoodleGradient(Port output, NodeEditor? input)
        {
            Color a, b;
            // If dragging the noodle, draw solid, slightly transparent
            if (input == null)
            {
                a = GetTypeColor(output.ValueType);
                b = a * new Color(1, 1, 1, 0.6f);
            }
            // If normal, draw gradient fading from one input color to the other
            else
            {
                a = GetTypeColor(output.ValueType);
                b = input.GetTint();
                if (output.Direction == IO.Input)
                {
                    (a, b) = (b, a);
                }

                // If any port is hovered, tint white
                if (_hoveredPort == output || output.NodeEditor == _hoveredNode || _hoveredNode == input)
                {
                    a = Color.Lerp(a, Color.white, 0.8f);
                    b = Color.Lerp(b, Color.white, 0.8f);
                }
            }

            if (s_gradientCache.TryGetValue((a, b), out var grad) == false)
                s_gradientCache[(a, b)] = grad = new Gradient
                {
                    colorKeys = new GradientColorKey[] { new GradientColorKey(a, 0f), new GradientColorKey(b, 1f) },
                    alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) },
                };

            return grad;
        }

        /// <summary> Returned float is used for noodle thickness </summary>
        /// <param name="output"> The output this noodle comes from. Never null. </param>
        /// <param name="input"> The output this noodle comes from. Can be null if we are dragging the noodle. </param>
        public virtual float GetNoodleThickness(Port output, NodeEditor? input)
        {
            return Preferences.GetSettings().NoodleThickness;
        }

        public virtual NoodlePath GetNoodlePath(Port output, NodeEditor? input)
        {
            return Preferences.GetSettings().NoodlePath;
        }

        /// <summary> Returned color is used to color ports </summary>
        public virtual Color GetPortColor(Port port)
        {
            return GetTypeColor(port.ValueType);
        }

        /// <summary>
        /// The returned Style is used to configure the paddings and icon texture of the ports.
        /// Use these properties to customize your port style.
        ///
        /// The properties used is:
        /// <see cref="GUIStyle.padding"/>[Left and Right], <see cref="GUIStyle.normal"/> [Background] = border texture,
        /// and <seealso cref="GUIStyle.active"/> [Background] = dot texture;
        /// </summary>
        /// <param name="port">the owner of the style</param>
        /// <returns></returns>
        public virtual GUIStyle GetPortStyle(Port port)
        {
            if (port.Direction == IO.Input)
                return Resources.Styles.InputPort;

            return Resources.Styles.OutputPort;
        }

        /// <summary> The returned color is used to color the background of the door.
        /// Usually used for outer edge effect </summary>
        public virtual Color GetPortBackgroundColor(Port port)
        {
            return Color.gray;
        }

        /// <summary> Returns generated color for a type. This color is editable in preferences </summary>
        public virtual Color GetTypeColor(Type type)
        {
            return Preferences.GetTypeColor(type);
        }

        /// <summary> Override to display custom tooltips </summary>
        public virtual string GetPortTooltip(Port port)
        {
            return port.Tooltip;
        }

        /// <summary> Deal with objects dropped into the graph through DragAndDrop </summary>
        public virtual void OnDropObjects(UnityEngine.Object[] objects)
        {
            Debug.Log("No OnDropObjects override defined for " + GetType());
        }

        /// <summary> Create a node and save it in the graph asset </summary>
        public virtual NodeEditor CreateNode(Type type, Vector2 position)
        {
            Undo.RecordObject(Graph, "Create Node");
            var node = Graph.AddNode(type);
            EditorUtility.SetDirty(Graph);

            var editor = InitNodeEditorFor(node);

            Undo.RegisterCreatedObjectUndo(editor, "Create Node");
            node.Position = position;
            Repaint();
            return editor;
        }

        NodeEditor InitNodeEditorFor(INodeValue node)
        {
            if (_nodesToEditor.TryGetValue(node, out var nodeEditor))
                return nodeEditor;

            var editor = (NodeEditor)CreateInstance(Utilities.GetCustomEditor(node.GetType(), typeof(CustomNodeEditor<>), typeof(NodeEditor)));
            editor.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
            editor.Value = node;
            editor.SerializedObject = new SerializedObject(editor);
            editor.ObjectTree = PropertyTree.Create(editor.SerializedObject);
            editor.Window = this;
            _nodesToEditor.Add(node, editor);
            return editor;
        }

        /// <summary> Creates a copy of the original node in the graph </summary>
        public virtual NodeEditor CopyNode(INodeValue original)
        {
            Undo.RecordObject(Graph, "Duplicate Node");
            var node = Graph.CopyNode(original);
            EditorUtility.SetDirty(Graph);
            var editor = InitNodeEditorFor(node);

            Undo.RegisterCreatedObjectUndo(editor, "Duplicate Node");
            Repaint();
            return editor;
        }

        /// <summary> Return false for nodes that can't be removed </summary>
        public virtual bool CanRemove(NodeEditor nodeEditor)
        {
            // Check graph attributes to see if this node is required
            Type graphType = Graph.GetType();
            RequireNodeAttribute[] attribs = Array.ConvertAll(
                graphType.GetCustomAttributes(typeof(RequireNodeAttribute), true),
                x => (RequireNodeAttribute)x);
            if (attribs.Any(x => x.Requires(nodeEditor.GetType())))
            {
                if (Graph.Nodes.Count(x => x.GetType() == nodeEditor.GetType()) <= 1)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary> Safely remove a node and all its connections. </summary>
        public virtual void RemoveNode(NodeEditor nodeEditor)
        {
            if (!CanRemove(nodeEditor)) return;

            // Remove the node
            Undo.RecordObject(nodeEditor, "Delete Node");
            Undo.RecordObject(Graph, "Delete Node");
            foreach (var editor in _nodesToEditor)
            {
                foreach (var port in editor.Value.Ports)
                {
                    if (port.Value.Connection == nodeEditor)
                        port.Value.Disconnect();
                }
            }
            Graph.RemoveNode(nodeEditor.Value);
            EditorUtility.SetDirty(Graph);
            _nodesToEditor.Remove(nodeEditor.Value);
            nodeEditor.ObjectTree.Dispose();
            Undo.DestroyObjectImmediate(nodeEditor);
        }
    }
}
