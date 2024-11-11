using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YNode.Editor.Internal;
#if UNITY_2019_1_OR_NEWER && USE_ADVANCED_GENERIC_MENU
using GenericMenu = XNodeEditor.AdvancedGenericMenu;
#endif

namespace YNode.Editor
{
    public partial class GraphWindow
    {
        public static NodeActivity CurrentActivity = NodeActivity.Idle;
        public static Vector2[] DragOffset = Array.Empty<Vector2>();
        public static NodeEditor[] CopyBuffer = Array.Empty<NodeEditor>();

        [NonSerialized] private Port? _draggedPort = null;
        [NonSerialized] private NodeEditor? _draggedOutputTarget = null;
        [NonSerialized] private NodeEditor? _hoveredNode = null;
        [NonSerialized] private Port? _hoveredPort = null;
        [NonSerialized] private ReroutePoint? _hoveredReroute = null;

        [NonSerialized] private Vector2 _dragBoxStart;
        [NonSerialized] private Rect _selectionBox;

        [NonSerialized] private bool _isDoubleClick = false;
        [NonSerialized] private Vector2 _lastMousePosition;
        [NonSerialized] private UnityEngine.Object[] _preBoxSelection = Array.Empty<UnityEngine.Object>();
        [NonSerialized] private ReroutePoint[] _preBoxSelectionReroute = Array.Empty<ReroutePoint>();
        [NonSerialized] private List<ReroutePoint> _selectedReroutes = new();

        /// <summary> Return the dragged port or null if not exist </summary>
        public Port? DraggedPort => _draggedPort;

        /// <summary> Return the Hovered port or null if not exist </summary>
        public Port? HoveredPort => _hoveredPort;

        /// <summary> Return the Hovered node or null if not exist </summary>
        public NodeEditor? HoveredNode => _hoveredNode;

        protected virtual void ControlsPreDraw()
        {
            wantsMouseMove = true;
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.MouseDrag: // Do this as a pre-step to consume the event and prevent fields of nodes from receiving the drag event
                    if (e.button == 0)
                    {
                        if (_draggedPort != null)
                        {
                            // Set target even if we can't connect, so as to prevent auto-conn menu from opening erroneously
                            if (_hoveredNode != null && !_draggedPort.Connection == _hoveredNode && _draggedPort.CanConnectTo(_hoveredNode.Value.GetType()))
                            {
                                _draggedOutputTarget = _hoveredNode;
                            }
                            else
                            {
                                _draggedOutputTarget = null;
                            }

                            Repaint();
                            e.Use();
                        }
                        else if (CurrentActivity == NodeActivity.HoldNode)
                        {
                            RecalculateDragOffsets(e);
                            CurrentActivity = NodeActivity.DragNode;
                            Repaint();
                            e.Use();
                        }

                        if (CurrentActivity == NodeActivity.DragNode)
                        {
                            // Holding ctrl inverts grid snap
                            bool gridSnap = Preferences.GetSettings().GridSnap;
                            if (e.control)
                                gridSnap = !gridSnap;

                            Vector2 mousePos = WindowToGridPosition(e.mousePosition);
                            // Move selected nodes with offset
                            for (int i = 0; i < Selection.objects.Length; i++)
                            {
                                if (Selection.objects[i] is NodeEditor node)
                                {
                                    Undo.RecordObject(node, "Moved Node");
                                    Vector2 initial = node.Value.Position;
                                    node.Value.Position = mousePos + DragOffset[i];
                                    if (gridSnap)
                                    {
                                        node.Value.Position = new(
                                            (Mathf.Round((node.Value.Position.x + 8) / 16) * 16) - 8,
                                            (Mathf.Round((node.Value.Position.y + 8) / 16) * 16) - 8);
                                    }

                                    // Offset portConnectionPoints instantly if a node is dragged so they aren't delayed by a frame.
                                    Vector2 offset = node.Value.Position - initial;
                                    if (offset.sqrMagnitude > 0)
                                    {
                                        foreach (var (_, port) in node.Ports)
                                        {
                                            Rect rect = port.CachedRect;
                                            rect.position += offset;
                                            port.CachedRect = rect;
                                        }
                                    }
                                }
                            }

                            // Move selected reroutes with offset
                            for (int i = 0; i < _selectedReroutes.Count; i++)
                            {
                                Vector2 pos = mousePos + DragOffset[Selection.objects.Length + i];
                                if (gridSnap)
                                {
                                    pos.x = (Mathf.Round(pos.x / 16) * 16);
                                    pos.y = (Mathf.Round(pos.y / 16) * 16);
                                }

                                _selectedReroutes[i].SetPoint(pos);
                            }

                            Repaint();
                            e.Use();
                            GUI.changed = true;
                        }
                        else if (CurrentActivity == NodeActivity.HoldGrid)
                        {
                            CurrentActivity = NodeActivity.BoxSelect;
                            _preBoxSelection = Selection.objects;
                            _preBoxSelectionReroute = _selectedReroutes.ToArray();
                            _dragBoxStart = WindowToGridPosition(e.mousePosition);
                            Repaint();
                            e.Use();
                        }
                        else if (CurrentActivity == NodeActivity.BoxSelect)
                        {
                            Vector2 boxStartPos = GridToWindowPosition(_dragBoxStart);
                            Vector2 boxSize = e.mousePosition - boxStartPos;
                            if (boxSize.x < 0)
                            {
                                boxStartPos.x += boxSize.x;
                                boxSize.x = Mathf.Abs(boxSize.x);
                            }

                            if (boxSize.y < 0)
                            {
                                boxStartPos.y += boxSize.y;
                                boxSize.y = Mathf.Abs(boxSize.y);
                            }

                            _selectionBox = new Rect(boxStartPos, boxSize);
                            Repaint();
                            e.Use();
                        }
                    }
                    break;
            }
        }

        protected virtual void ControlsPostDraw()
        {
            wantsMouseMove = true;
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                    if (e.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        OnDropObjects(DragAndDrop.objectReferences);
                        GUI.changed = true;
                    }

                    break;
                case EventType.MouseMove:
                    //Keyboard commands will not get correct mouse position from Event
                    _lastMousePosition = e.mousePosition;
                    break;
                case EventType.ScrollWheel:
                    float oldZoom = Zoom;
                    if (e.delta.y > 0)
                        Zoom += 0.1f * Zoom;
                    else
                        Zoom -= 0.1f * Zoom;
                    if (Preferences.GetSettings().ZoomToMouse)
                        PanOffset += (1 - oldZoom / Zoom) * (WindowToGridPosition(e.mousePosition) + PanOffset);
                    break;
                case EventType.MouseDrag:
                    if (e.button is 1 or 2 && e.delta != default && (CurrentActivity is NodeActivity.Idle or NodeActivity.DragGrid))
                    {
                        PanOffset += e.delta * Zoom;
                        CurrentActivity = NodeActivity.DragGrid;
                    }
                    break;
                case EventType.MouseDown:
                    Repaint();
                    if (e.button == 0)
                    {
                        if (_draggedPort is not null)
                        {
                            GUI.changed = true;
                            _draggedPort.ClearReroute();
                        }

                        if (_hoveredPort != null)
                        {
                            _draggedPort = _hoveredPort;
                            if (_hoveredPort.Connection is not null)
                            {
                                GUI.changed = true;
                                _hoveredPort.Disconnect();
                                _draggedOutputTarget = _hoveredPort.NodeEditor;
                            }
                        }
                        else if (_hoveredReroute is { } hoveredRerouteValue)
                        {
                            GUI.changed = true;
                            // If reroute isn't selected
                            if (!_selectedReroutes.Contains(hoveredRerouteValue))
                            {
                                if (e.control || e.shift) // Add it
                                {
                                    _selectedReroutes.Add(hoveredRerouteValue);
                                }
                                else // Select it
                                {
                                    _selectedReroutes = new List<ReroutePoint>() { hoveredRerouteValue };
                                    Selection.activeObject = null;
                                }
                            }
                            // Deselect
                            else if (e.control || e.shift)
                                _selectedReroutes.Remove(hoveredRerouteValue);

                            e.Use();
                            CurrentActivity = NodeActivity.HoldNode;
                        }
                        else if (_hoveredNode != null)
                        {
                            // If mousedown on node header, select or deselect
                            if (!Selection.Contains(_hoveredNode))
                            {
                                SelectNode(_hoveredNode, e.control || e.shift);
                                if (!e.control && !e.shift) _selectedReroutes.Clear();
                            }
                            else if (e.control || e.shift)
                                DeselectNode(_hoveredNode);

                            // Cache double click state, but only act on it in MouseUp - Except ClickCount only works in mouseDown.
                            _isDoubleClick = (e.clickCount == 2);

                            e.Use();
                            CurrentActivity = NodeActivity.HoldNode;
                        }
                        // If mousedown on grid background, deselect all
                        else if (_hoveredNode == null)
                        {
                            CurrentActivity = NodeActivity.HoldGrid;
                            if (!e.control && !e.shift)
                            {
                                _selectedReroutes.Clear();
                                Selection.activeObject = null;
                            }
                        }
                    }

                    break;
                case EventType.MouseUp:
                    if (e.button == 0)
                    {
                        //Port drag release
                        if (_draggedPort != null)
                        {
                            // If connection is valid, save it
                            if (_draggedOutputTarget != null && _draggedPort.CanConnectTo(_draggedOutputTarget.Value.GetType()))
                            {
                                _draggedPort.Connect(_draggedOutputTarget);
                            }
                            // Open context menu for auto-connection if there is no target node
                            else if (_draggedOutputTarget == null)
                            {
                                _draggedPort.ClearReroute();
                                if (Preferences.GetSettings().DragToCreate)
                                {
                                    GenericMenu menu = new GenericMenu();
                                    AddContextMenuItems(menu, _draggedPort.CanConnectTo, _draggedPort.Connect);
                                    menu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
                                }
                            }

                            //Release dragged connection
                            _draggedPort = null;
                            _draggedOutputTarget = null;
                            GUI.changed = true;
                        }
                        else if (CurrentActivity == NodeActivity.DragNode)
                        {
                            GUI.changed = true;
                        }
                        else if (_hoveredNode == null)
                        {
                            // If click outside node, release field focus
                            EditorGUI.FocusTextInControl(null);
                            EditorGUIUtility.editingTextField = false;
                        }

                        // If click node header, select it.
                        if (CurrentActivity == NodeActivity.HoldNode && !(e.control || e.shift))
                        {
                            _selectedReroutes.Clear();
                            if (_hoveredNode == null)
                                throw new Exception();
                            SelectNode(_hoveredNode, false);

                            // Double click to center node
                            if (_isDoubleClick)
                            {
                                Vector2 nodeDimension = _hoveredNode.CachedSize / 2;
                                PanOffset = -_hoveredNode.Value.Position - nodeDimension;
                            }
                        }

                        // If click reroute, select it.
                        if (_hoveredReroute is {} hoveredRerouteValue && !(e.control || e.shift))
                        {
                            _selectedReroutes = new List<ReroutePoint> { hoveredRerouteValue };
                            Selection.activeObject = null;
                        }

                        Repaint();
                        CurrentActivity = NodeActivity.Idle;
                    }
                    else if (e.button is 1 or 2 && CurrentActivity != NodeActivity.DragGrid)
                    {
                        if (_draggedPort != null)
                        {
                            _draggedPort.GetReroutePoints().Add(WindowToGridPosition(e.mousePosition));
                            GUI.changed = true;
                        }
                        else if (CurrentActivity == NodeActivity.DragNode && Selection.activeObject == null &&
                                 _selectedReroutes.Count == 1)
                        {
                            _selectedReroutes[0].InsertPoint(_selectedReroutes[0].GetPoint());
                            _selectedReroutes[0] = new ReroutePoint(_selectedReroutes[0].Port,
                                _selectedReroutes[0].PointIndex + 1);
                            GUI.changed = true;
                        }
                        else if (_hoveredReroute is {} hoveredRerouteValue)
                        {
                            ShowRerouteContextMenu(hoveredRerouteValue);
                        }
                        else if (_hoveredPort != null)
                        {
                            ShowPortContextMenu(_hoveredPort);
                        }
                        else if (_hoveredNode != null && IsHoveringTitle(_hoveredNode))
                        {
                            if (!Selection.Contains(_hoveredNode)) SelectNode(_hoveredNode, false);
                            GenericMenu menu = new GenericMenu();
                            _hoveredNode.AddContextMenuItems(menu);
                            menu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
                            e.Use(); // Fixes copy/paste context menu appearing in Unity 5.6.6f2 - doesn't occur in 2018.3.2f1 Probably needs to be used in other places.
                        }
                        else if (_hoveredNode == null)
                        {
                            GenericMenu menu = new GenericMenu();
                            AddContextMenuItems(menu, null, null);
                            menu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
                        }
                    }
                    else if (e.button is 1 or 2 && CurrentActivity == NodeActivity.DragGrid)
                        CurrentActivity = NodeActivity.Idle;

                    // Reset DoubleClick
                    _isDoubleClick = false;
                    break;
                case EventType.KeyDown:
                    if (EditorGUIUtility.editingTextField || GUIUtility.keyboardControl != 0)
                        break;

                    if (e.keyCode == KeyCode.F)
                    {
                        Home();
                    }
                    else if (e.keyCode == KeyCode.A)
                    {
                        if (Selection.objects.Any(x => x is NodeEditor n && _nodesToEditor.ContainsKey(n.Value)))
                        {
                            foreach (var (_, node) in _nodesToEditor)
                            {
                                DeselectNode(node);
                            }
                        }
                        else
                        {
                            foreach (var (_, node) in _nodesToEditor)
                            {
                                SelectNode(node, true);
                            }
                        }

                        Repaint();
                    }

                    break;
                case EventType.ValidateCommand:
                case EventType.ExecuteCommand:
                    if (e.commandName == "SoftDelete")
                    {
                        if (e.type == EventType.ExecuteCommand)
                        {
                            RemoveSelectedNodes();
                            GUI.changed = true;
                        }

                        e.Use();
                    }
                    else if (Utilities.IsMac() && e.commandName == "Delete")
                    {
                        if (e.type == EventType.ExecuteCommand)
                        {
                            RemoveSelectedNodes();
                            GUI.changed = true;
                        }

                        e.Use();
                    }
                    else if (e.commandName == "Duplicate")
                    {
                        if (e.type == EventType.ExecuteCommand)
                        {
                            DuplicateSelectedNodes();
                            GUI.changed = true;
                        }
                        e.Use();
                    }
                    else if (e.commandName == "Copy")
                    {
                        if (!EditorGUIUtility.editingTextField)
                        {
                            if (e.type == EventType.ExecuteCommand)
                            {
                                CopySelectedNodes();
                                GUI.changed = true;
                            }
                            e.Use();
                        }
                    }
                    else if (e.commandName == "Paste")
                    {
                        if (!EditorGUIUtility.editingTextField)
                        {
                            if (e.type == EventType.ExecuteCommand)
                            {
                                PasteNodes(WindowToGridPosition(_lastMousePosition));
                                GUI.changed = true;
                            }
                            e.Use();
                        }
                    }

                    Repaint();
                    break;
                case EventType.Ignore:
                    // If release mouse outside window
                    if (e.rawType == EventType.MouseUp && CurrentActivity == NodeActivity.BoxSelect)
                    {
                        Repaint();
                        CurrentActivity = NodeActivity.Idle;
                    }

                    break;
            }
        }

        private void RecalculateDragOffsets(Event current)
        {
            DragOffset = new Vector2[Selection.objects.Length + _selectedReroutes.Count];
            // Selected nodes
            for (int i = 0; i < Selection.objects.Length; i++)
            {
                if (Selection.objects[i] is NodeEditor node)
                {
                    DragOffset[i] = node.Value.Position - WindowToGridPosition(current.mousePosition);
                }
            }

            // Selected reroutes
            for (int i = 0; i < _selectedReroutes.Count; i++)
            {
                DragOffset[Selection.objects.Length + i] =
                    _selectedReroutes[i].GetPoint() - WindowToGridPosition(current.mousePosition);
            }
        }

        /// <summary> Puts all selected nodes in focus. If no nodes are present, resets view and zoom to to origin </summary>
        public void Home()
        {
            var nodes = Selection.objects.OfType<NodeEditor>().ToList();
            if (nodes.Count > 0)
            {
                Vector2 minPos = nodes.Select(x => x.Value.Position)
                    .Aggregate((x, y) => new Vector2(Mathf.Min(x.x, y.x), Mathf.Min(x.y, y.y)));
                Vector2 maxPos = nodes
                    .Select(x => x.Value.Position + x.CachedSize)
                    .Aggregate((x, y) => new Vector2(Mathf.Max(x.x, y.x), Mathf.Max(x.y, y.y)));
                PanOffset = -(minPos + (maxPos - minPos) / 2f);
            }
            else
            {
                Zoom = 2;
                PanOffset = Vector2.zero;
            }
        }

        /// <summary> Remove nodes in the graph in Selection.objects</summary>
        public void RemoveSelectedNodes()
        {
            // We need to delete reroutes starting at the highest point index to avoid shifting indices
            _selectedReroutes = _selectedReroutes.OrderByDescending(x => x.PointIndex).ToList();
            for (int i = 0; i < _selectedReroutes.Count; i++)
            {
                _selectedReroutes[i].RemovePoint();
            }

            _selectedReroutes.Clear();
            foreach (UnityEngine.Object item in Selection.objects)
            {
                if (item is NodeEditor node)
                {
                    RemoveNode(node);
                }
            }
        }

        /// <summary> Draw this node on top of other nodes by placing it last in the graph.nodes list </summary>
        public void MoveNodeToTop(NodeEditor nodeEditor)
        {
            var val = nodeEditor.Value;
            var index = Graph.Nodes.IndexOf(val);
            Graph.Nodes.RemoveAt(index);
            Graph.Nodes.Add(val);
        }

        /// <summary> Duplicate selected nodes and select the duplicates </summary>
        public void DuplicateSelectedNodes()
        {
            // Get selected nodes which are part of this graph
            NodeEditor[] selectedNodes = Selection.objects.OfType<NodeEditor>().Where(x => x.Graph == Graph).ToArray();
            if (selectedNodes.Length == 0) return;
            // Get top left node position
            Vector2 topLeftNode = selectedNodes.Select(x => x.Value.Position)
                .Aggregate((x, y) => new Vector2(Mathf.Min(x.x, y.x), Mathf.Min(x.y, y.y)));
            InsertDuplicateNodes(selectedNodes, topLeftNode + new Vector2(30, 30));
        }

        public void CopySelectedNodes()
        {
            CopyBuffer = Selection.objects.OfType<NodeEditor>().Where(x => x.Graph == Graph).ToArray();
        }

        public void PasteNodes(Vector2 pos)
        {
            InsertDuplicateNodes(CopyBuffer, pos);
        }

        private void InsertDuplicateNodes(NodeEditor[] nodes, Vector2 topLeft)
        {
            if (nodes.Length == 0) return;

            // Get top-left node
            Vector2 topLeftNode = nodes.Select(x => x.Value.Position)
                .Aggregate((x, y) => new Vector2(Mathf.Min(x.x, y.x), Mathf.Min(x.y, y.y)));
            Vector2 offset = topLeft - topLeftNode;

            UnityEngine.Object[] newNodes = new UnityEngine.Object[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                NodeEditor srcNodeEditor = nodes[i];
                if (srcNodeEditor == null) continue;

                // Check if user is allowed to add more of given node type
                Type nodeType = srcNodeEditor.GetType();
                if (Utilities.GetAttrib<DisallowMultipleNodesAttribute>(nodeType, out var disallowAttrib))
                {
                    int typeCount = Graph.Nodes.Count(x => x.GetType() == nodeType);
                    if (typeCount >= disallowAttrib.max) continue;
                }

                NodeEditor newNodeEditor = CopyNode(srcNodeEditor.Value);
                newNodeEditor.Value.Position = srcNodeEditor.Value.Position + offset;
                newNodes[i] = newNodeEditor;
            }

            EditorUtility.SetDirty(Graph);
            // Select the new nodes
            Selection.objects = newNodes;
        }

        /// <summary> Draw a connection as we are dragging it </summary>
        public void DrawDraggedConnection()
        {
            if (_draggedPort == null)
                return;

            Gradient gradient = GetNoodleGradient(_draggedPort, _draggedOutputTarget);
            float thickness = GetNoodleThickness(_draggedPort, _draggedOutputTarget);
            NoodlePath path = GetNoodlePath(_draggedPort, _draggedOutputTarget);
            NoodleStroke stroke = _draggedPort.Stroke;

            if (_draggedPort.CachedRect == default)
                return;

            Rect fromRect = _draggedPort.CachedRect;
            var gridPoints = new List<Vector2>();
            gridPoints.Add(fromRect.center);
            if (_draggedPort.TryGetReroutePoints(out var reroute))
                gridPoints.AddRange(reroute);

            Vector2 endPoint;
            if (_draggedOutputTarget != null)
                endPoint = GetNodeEndpointPosition(_draggedOutputTarget, _draggedPort.Direction);
            else
                endPoint = WindowToGridPosition(Event.current.mousePosition);

            gridPoints.Add(endPoint);

            bool isInput = _draggedPort.Direction == IO.Input;
            if (isInput)
                gridPoints.Reverse();

            DrawNoodle(gradient, path, stroke, thickness, gridPoints);
            DrawArrow(_draggedPort.Direction, endPoint, gradient.colorKeys[isInput ? 0 : ^1].color);

            GUIStyle portStyle = GetPortStyle(_draggedPort);
            Color bgcol = Color.black;
            Color frcol = gradient.colorKeys[0].color;
            bgcol.a = 0.6f;
            frcol.a = 0.6f;

            if (_draggedPort.TryGetReroutePoints(out reroute))
            {
                // Loop through reroute points again and draw the points
                for (int i = 0; i < reroute.Count; i++)
                {
                    // Draw reroute point at position
                    Rect rect = new Rect(reroute[i], new Vector2(16, 16));
                    rect.position = new Vector2(rect.position.x - 8, rect.position.y - 8);
                    rect = GridToWindowRect(rect);

                    DrawPortHandle(rect, bgcol, frcol, portStyle.normal.background, portStyle.active.background);
                }
            }
        }

        private bool IsHoveringTitle(NodeEditor nodeEditor)
        {
            Vector2 mousePos = Event.current.mousePosition;
            //Get node position
            Vector2 nodePos = GridToWindowPosition(nodeEditor.Value.Position);
            float width = nodeEditor.CachedSize.x == 0 ? 200 : nodeEditor.CachedSize.x;
            Rect windowRect = new Rect(nodePos, new Vector2(width / Zoom, 30 / Zoom));
            return windowRect.Contains(mousePos);
        }

        private Vector2 GetNodeEndpointPosition(NodeEditor nodeEditor, IO direction)
        {
            Vector2 pos;
            if (_stickyEditors.Contains(nodeEditor))
                pos = GetStickyGridPosition(nodeEditor);
            else
                pos = nodeEditor.Value.Position;

            if (direction == IO.Input)
                pos += new Vector2(nodeEditor.GetWidth() + ArrowWidth, 20);
            else
                pos += new Vector2(-ArrowWidth, 20);


            return pos;
        }
    }
}
