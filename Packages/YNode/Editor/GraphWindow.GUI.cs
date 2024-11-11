using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using YNode.Editor.Internal;
using Object = UnityEngine.Object;
#if UNITY_2019_1_OR_NEWER && USE_ADVANCED_GENERIC_MENU
using GenericMenu = XNodeEditor.AdvancedGenericMenu;
#endif

namespace YNode.Editor
{
    /// <summary> Contains GUI methods </summary>
    public partial class GraphWindow
    {
        private const float ArrowWidth = 16;
        private static readonly Vector3[] s_polyLineTempArray = new Vector3[2];
        private HashSet<NodeEditor> _culledEditors = new();
        private HashSet<NodeEditor> _stickyEditors = new();
        private bool _firstRun = true;
        [NonSerialized] private string? _title, _titleModified;

        /// <summary> 19 if docked, 21 if not </summary>
        private int TopPadding => IsDocked() ? 19 : 21;
        private DateTime? _lastChange;

        /// <summary> Executed after all other window GUI. Useful if Zoom is ruining your day. Automatically resets after being run.</summary>
        public event Action? OnLateGUI;

        protected virtual void OnGUI()
        {
            if (_ranLoad == false)
            {
                _firstRun = true;
                Load();
            }

            Current = this;
            if (Graph == null)
                return;

            _title ??= Graph.name;
            _titleModified ??= $"{_title}*";
            titleContent.text = EditorUtility.IsDirty(Graph) ? _titleModified : _title;

            Matrix4x4 m = GUI.matrix;

            EditorGUI.BeginChangeCheck();

            ControlsPreDraw();
            DrawGrid(position, Zoom, PanOffset);
            DrawConnections();
            DrawDraggedConnection();
            DrawNodes();
            DrawSelectionBox();
            DrawTooltip();
            OnGUIOverlay();
            ControlsPostDraw();

            // Run and reset onLateGUI
            if (OnLateGUI != null)
            {
                OnLateGUI();
                OnLateGUI = null;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(Graph);
                if (Preferences.GetSettings().AutoSave)
                {
                    if (_lastChange == null)
                        EditorApplication.update += AutoSave;
                    _lastChange = DateTime.Now;
                }
            }

            GUI.matrix = m;
            if (Event.current.type == EventType.Repaint)
                _firstRun = false;
        }

        void AutoSave()
        {
            if (_lastChange is null)
                throw new InvalidOperationException();

            if ((DateTime.Now - _lastChange.Value).Seconds > 3)
            {
                Save();
                EditorApplication.update -= AutoSave;
                _lastChange = null;
            }
        }

        protected virtual void OnGUIOverlay() { }

        private static void BeginZoomed(Rect rect, float zoom, float topPadding)
        {
            GUI.EndClip();

            GUIUtility.ScaleAroundPivot(Vector2.one / zoom, rect.size * 0.5f);
            GUI.BeginClip(new Rect(-((rect.width * zoom) - rect.width) * 0.5f,
                -(((rect.height * zoom) - rect.height) * 0.5f) + (topPadding * zoom),
                rect.width * zoom,
                rect.height * zoom));
        }

        private static void EndZoomed(Rect rect, float zoom, float topPadding)
        {
            GUIUtility.ScaleAroundPivot(Vector2.one * zoom, rect.size * 0.5f);
            Vector3 offset = new Vector3(
                (((rect.width * zoom) - rect.width) * 0.5f),
                (((rect.height * zoom) - rect.height) * 0.5f) + (-topPadding * zoom) + topPadding,
                0);
            GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one);
        }

        private void DrawGrid(Rect rect, float zoom, Vector2 panOffset)
        {
            rect.position = Vector2.zero;

            Vector2 center = rect.size / 2f;
            Texture2D gridTex = GetGridTexture();
            Texture2D crossTex = GetSecondaryGridTexture();

            // Offset from origin in tile units
            float xOffset = -(center.x * zoom + panOffset.x) / gridTex.width;
            float yOffset = ((center.y - rect.size.y) * zoom + panOffset.y) / gridTex.height;

            Vector2 tileOffset = new Vector2(xOffset, yOffset);

            // Amount of tiles
            float tileAmountX = Mathf.Round(rect.size.x * zoom) / gridTex.width;
            float tileAmountY = Mathf.Round(rect.size.y * zoom) / gridTex.height;

            Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

            // Draw tiled background
            GUI.DrawTextureWithTexCoords(rect, gridTex, new Rect(tileOffset, tileAmount));
            GUI.DrawTextureWithTexCoords(rect, crossTex, new Rect(tileOffset + new Vector2(0.5f, 0.5f), tileAmount));
        }

        private void DrawSelectionBox()
        {
            if (CurrentActivity == NodeActivity.BoxSelect)
            {
                Vector2 curPos = WindowToGridPosition(Event.current.mousePosition);
                Vector2 size = curPos - _dragBoxStart;
                Rect r = new Rect(_dragBoxStart, size);
                r.position = GridToWindowPosition(r.position);
                r.size /= Zoom;
                Handles.DrawSolidRectangleWithOutline(r, new Color(0, 0, 0, 0.1f), new Color(1, 1, 1, 0.6f));
            }
        }

        /// <summary> Show right-click context menu for hovered reroute </summary>
        private void ShowRerouteContextMenu(ReroutePoint reroute)
        {
            GenericMenu contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Remove"), false, () => reroute.RemovePoint());
            contextMenu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
        }

        /// <summary> Show right-click context menu for hovered port </summary>
        private void ShowPortContextMenu(Port hoveredPort)
        {
            GenericMenu contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Clear Connections"), false, hoveredPort.Disconnect);
            //Get compatible nodes with this port
            if (Preferences.GetSettings().CreateFilter)
            {
                contextMenu.AddSeparator("");
                AddContextMenuItems(contextMenu, hoveredPort.CanConnectTo, hoveredPort.Connect);
            }

            contextMenu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
        }

        private static Vector2 CalculateBezierPoint(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1 - t;
            float tt = t * t, uu = u * u;
            float uuu = uu * u, ttt = tt * t;
            return new Vector2(
                (uuu * p0.x) + (3 * uu * t * p1.x) + (3 * u * tt * p2.x) + (ttt * p3.x),
                (uuu * p0.y) + (3 * uu * t * p1.y) + (3 * u * tt * p2.y) + (ttt * p3.y)
            );
        }

        /// <summary> Draws a line segment without allocating temporary arrays </summary>
        private static void DrawAAPolyLineNonAlloc(float thickness, Vector2 p0, Vector2 p1)
        {
            s_polyLineTempArray[0].x = p0.x;
            s_polyLineTempArray[0].y = p0.y;
            s_polyLineTempArray[1].x = p1.x;
            s_polyLineTempArray[1].y = p1.y;
            Handles.DrawAAPolyLine(thickness, s_polyLineTempArray);
        }

        /// <summary> Draws a line segment with shadows without allocating temporary arrays </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DrawAAPolyLineWithShadowNonAlloc(float thickness, Vector2 p0, Vector2 p1)
        {
            s_polyLineTempArray[0].x = p0.x;
            s_polyLineTempArray[0].y = p0.y;
            s_polyLineTempArray[1].x = p1.x;
            s_polyLineTempArray[1].y = p1.y;
            var previousColor = Handles.color;
            Handles.color = Color.black;
            Handles.DrawAAPolyLine(thickness*1.5f, s_polyLineTempArray);
            Handles.color = previousColor;
            Handles.DrawAAPolyLine(thickness, s_polyLineTempArray);
        }

        /// <summary> Draw a bezier from output to input in grid coordinates </summary>
        public void DrawNoodle(Gradient gradient, NoodlePath path, NoodleStroke stroke, float thickness, List<Vector2> gridPoints)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            // convert grid points to window points
            for (int i = 0; i < gridPoints.Count; ++i)
                gridPoints[i] = GridToWindowPosition(gridPoints[i]);

            Color originalHandlesColor = Handles.color;
            Handles.color = gradient.Evaluate(0f);
            int length = gridPoints.Count;
            switch (path)
            {
                case NoodlePath.Curvy:
                    Vector2 outputTangent = Vector2.right;
                    for (int i = 0; i < length - 1; i++)
                    {
                        Vector2 inputTangent;
                        // Cached most variables that repeat themselves here to avoid so many indexer calls :p
                        Vector2 pointA = gridPoints[i];
                        Vector2 pointB = gridPoints[i + 1];
                        float distAb = Vector2.Distance(pointA, pointB);
                        if (i == 0) outputTangent = Zoom * distAb * 0.01f * Vector2.right;
                        if (i < length - 2)
                        {
                            Vector2 pointC = gridPoints[i + 2];
                            Vector2 ab = (pointB - pointA).normalized;
                            Vector2 cb = (pointB - pointC).normalized;
                            Vector2 ac = (pointC - pointA).normalized;
                            Vector2 p = (ab + cb) * 0.5f;
                            float tangentLength = (distAb + Vector2.Distance(pointB, pointC)) * 0.005f * Zoom;
                            float side = ((ac.x * (pointB.y - pointA.y)) - (ac.y * (pointB.x - pointA.x)));

                            p = tangentLength * Mathf.Sign(side) * new Vector2(-p.y, p.x);
                            inputTangent = p;
                        }
                        else
                        {
                            inputTangent = Zoom * distAb * 0.01f * Vector2.left;
                        }

                        // Calculates the tangents for the bezier's curves.
                        float zoomCoef = 50 / Zoom;
                        Vector2 tangentA = pointA + outputTangent * zoomCoef;
                        Vector2 tangentB = pointB + inputTangent * zoomCoef;
                        // Hover effect.
                        int division = Mathf.RoundToInt(.2f * distAb) + 3;
                        // Coloring and bezier drawing.
                        int draw = 0;
                        Vector2 bezierPrevious = pointA;
                        for (int j = 1; j <= division; ++j)
                        {
                            if (stroke == NoodleStroke.Dashed)
                            {
                                draw++;
                                if (draw >= 2) draw = -2;
                                if (draw < 0) continue;
                                if (draw == 0)
                                    bezierPrevious = CalculateBezierPoint(pointA, tangentA, tangentB, pointB, (j - 1f) / (float)division);
                            }

                            if (i == length - 2)
                                Handles.color = gradient.Evaluate((j + 1f) / division);
                            Vector2 bezierNext = CalculateBezierPoint(pointA, tangentA, tangentB, pointB, j / (float)division);
                            DrawAAPolyLineWithShadowNonAlloc(thickness, bezierPrevious, bezierNext);
                            bezierPrevious = bezierNext;
                        }

                        outputTangent = -inputTangent;
                    }

                    break;
                case NoodlePath.Straight:
                    for (int i = 0; i < length - 1; i++)
                    {
                        Vector2 pointA = gridPoints[i];
                        Vector2 pointB = gridPoints[i + 1];
                        // Draws the line with the coloring.
                        Vector2 prevPoint = pointA;
                        // Approximately one segment per 5 pixels
                        int segments = (int)Vector2.Distance(pointA, pointB) / 5;
                        segments = Math.Max(segments, 1);

                        int draw = 0;
                        for (int j = 0; j <= segments; j++)
                        {
                            draw++;
                            float t = j / (float)segments;
                            Vector2 lerp = Vector2.Lerp(pointA, pointB, t);
                            if (draw > 0)
                            {
                                if (i == length - 2) Handles.color = gradient.Evaluate(t);
                                DrawAAPolyLineWithShadowNonAlloc(thickness, prevPoint, lerp);
                            }

                            prevPoint = lerp;
                            if (stroke == NoodleStroke.Dashed && draw >= 2) draw = -2;
                        }
                    }

                    break;
                case NoodlePath.Angled:
                    for (int i = 0; i < length - 1; i++)
                    {
                        if (i == length - 1) continue; // Skip last index
                        if (gridPoints[i].x <= gridPoints[i + 1].x - (50 / Zoom))
                        {
                            float midpoint = (gridPoints[i].x + gridPoints[i + 1].x) * 0.5f;
                            Vector2 start1 = gridPoints[i];
                            Vector2 end1 = gridPoints[i + 1];
                            start1.x = midpoint;
                            end1.x = midpoint;
                            if (i == length - 2)
                            {
                                DrawAAPolyLineWithShadowNonAlloc(thickness, gridPoints[i], start1);
                                Handles.color = gradient.Evaluate(0.5f);
                                DrawAAPolyLineWithShadowNonAlloc(thickness, start1, end1);
                                Handles.color = gradient.Evaluate(1f);
                                DrawAAPolyLineWithShadowNonAlloc(thickness, end1, gridPoints[i + 1]);
                            }
                            else
                            {
                                DrawAAPolyLineWithShadowNonAlloc(thickness, gridPoints[i], start1);
                                DrawAAPolyLineWithShadowNonAlloc(thickness, start1, end1);
                                DrawAAPolyLineWithShadowNonAlloc(thickness, end1, gridPoints[i + 1]);
                            }
                        }
                        else
                        {
                            float midpoint = (gridPoints[i].y + gridPoints[i + 1].y) * 0.5f;
                            Vector2 start1 = gridPoints[i];
                            Vector2 end1 = gridPoints[i + 1];
                            start1.x += 25 / Zoom;
                            end1.x -= 25 / Zoom;
                            Vector2 start2 = start1;
                            Vector2 end2 = end1;
                            start2.y = midpoint;
                            end2.y = midpoint;
                            if (i == length - 2)
                            {
                                DrawAAPolyLineWithShadowNonAlloc(thickness, gridPoints[i], start1);
                                Handles.color = gradient.Evaluate(0.25f);
                                DrawAAPolyLineWithShadowNonAlloc(thickness, start1, start2);
                                Handles.color = gradient.Evaluate(0.5f);
                                DrawAAPolyLineWithShadowNonAlloc(thickness, start2, end2);
                                Handles.color = gradient.Evaluate(0.75f);
                                DrawAAPolyLineWithShadowNonAlloc(thickness, end2, end1);
                                Handles.color = gradient.Evaluate(1f);
                                DrawAAPolyLineWithShadowNonAlloc(thickness, end1, gridPoints[i + 1]);
                            }
                            else
                            {
                                DrawAAPolyLineWithShadowNonAlloc(thickness, gridPoints[i], start1);
                                DrawAAPolyLineWithShadowNonAlloc(thickness, start1, start2);
                                DrawAAPolyLineWithShadowNonAlloc(thickness, start2, end2);
                                DrawAAPolyLineWithShadowNonAlloc(thickness, end2, end1);
                                DrawAAPolyLineWithShadowNonAlloc(thickness, end1, gridPoints[i + 1]);
                            }
                        }
                    }

                    break;
                case NoodlePath.ShaderLab:
                    Vector2 start = gridPoints[0];
                    Vector2 end = gridPoints[length - 1];
                    //Modify first and last point in array so we can loop trough them nicely.
                    gridPoints[0] = gridPoints[0] + Vector2.right * (20 / Zoom);
                    gridPoints[length - 1] = gridPoints[length - 1] + Vector2.left * (20 / Zoom);
                    //Draw first vertical lines going out from nodes
                    Handles.color = gradient.Evaluate(0f);
                    DrawAAPolyLineWithShadowNonAlloc(thickness, start, gridPoints[0]);
                    Handles.color = gradient.Evaluate(1f);
                    DrawAAPolyLineWithShadowNonAlloc(thickness, end, gridPoints[length - 1]);
                    for (int i = 0; i < length - 1; i++)
                    {
                        Vector2 pointA = gridPoints[i];
                        Vector2 pointB = gridPoints[i + 1];
                        // Draws the line with the coloring.
                        Vector2 prevPoint = pointA;
                        // Approximately one segment per 5 pixels
                        int segments = (int)Vector2.Distance(pointA, pointB) / 5;
                        segments = Math.Max(segments, 1);

                        int draw = 0;
                        for (int j = 0; j <= segments; j++)
                        {
                            draw++;
                            float t = j / (float)segments;
                            Vector2 lerp = Vector2.Lerp(pointA, pointB, t);
                            if (draw > 0)
                            {
                                if (i == length - 2) Handles.color = gradient.Evaluate(t);
                                DrawAAPolyLineWithShadowNonAlloc(thickness, prevPoint, lerp);
                            }

                            prevPoint = lerp;
                            if (stroke == NoodleStroke.Dashed && draw >= 2) draw = -2;
                        }
                    }

                    gridPoints[0] = start;
                    gridPoints[length - 1] = end;
                    break;
            }

            Handles.color = originalHandlesColor;
        }

        /// <summary> Draws all connections </summary>
        public void DrawConnections()
        {
            Vector2 mousePos = Event.current.mousePosition;
            List<ReroutePoint> selection = new List<ReroutePoint>(_preBoxSelectionReroute);
            _hoveredReroute = null;

            if (Event.current.type == EventType.Layout)
                _hoveredPort = null;

            List<Vector2> gridPoints = new List<Vector2>(2);

            Color col = GUI.color;
            foreach ((_, NodeEditor node) in _nodesToEditor)
            {
                // Draw full connections and output > reroute
                foreach ((_, Port port) in node.Ports)
                {
                    //Needs cleanup. Null checks are ugly
                    Rect fromRect = port.CachedRect;
                    if (fromRect == default)
                        continue;

                    Color portColor = GetPortColor(port);
                    GUIStyle portStyle = GetPortStyle(port);

                    var portRect = fromRect;
                    Color backgroundColor = GetPortBackgroundColor(port);

                    var portRectInWindowSpace = GridToWindowRect(portRect);

                    if (portRectInWindowSpace.Contains(mousePos))
                        _hoveredPort = port;

                    if (port.Connection is {} target)
                    {
                        var endPosition = GetNodeEndpointPosition(target, port.Direction);
                        var toRect = new Rect(endPosition, default);
                        if (port.Direction == IO.Input)
                            (fromRect, toRect) = (toRect, fromRect);

                        port.TryGetReroutePoints(out var reroutePoints);

                        gridPoints.Clear();
                        gridPoints.Add(fromRect.center);
                        if (reroutePoints != null)
                            gridPoints.AddRange(reroutePoints);
                        gridPoints.Add(toRect.center);

                        Vector2 min = toRect.center;
                        Vector2 max = toRect.center;
                        foreach (var point in gridPoints)
                        {
                            min = Vector2.Min(min, point);
                            max = Vector2.Max(max, point);
                        }
                        Rect boundingBox = default;
                        boundingBox.min = min;
                        boundingBox.max = max;

                        if (ShouldBeCulled(boundingBox) == false)
                        {
                            NoodleStroke noodleStroke = port.Stroke;
                            float noodleThickness = GetNoodleThickness(port, target);
                            NoodlePath noodlePath = GetNoodlePath(port, target);
                            Gradient noodleGradient = GetNoodleGradient(port, target);

                            var arrowRect = DrawArrow(port.Direction, endPosition, noodleGradient.Evaluate(port.Direction == IO.Input ? 0 : 1));
                            if (arrowRect.Contains(mousePos))
                                _hoveredPort = port;
                            DrawNoodle(noodleGradient, noodlePath, noodleStroke, noodleThickness, gridPoints);

                            if (reroutePoints != null)
                            {
                                // Loop through reroute points again and draw the points
                                for (int i = 0; i < reroutePoints.Count; i++)
                                {
                                    ReroutePoint rerouteRef = new ReroutePoint(port, i);
                                    // Draw reroute point at position
                                    Rect rect = new Rect(reroutePoints[i], new Vector2(12, 12));
                                    rect.position = new Vector2(rect.position.x - 6, rect.position.y - 6);
                                    rect = GridToWindowRect(rect);

                                    // Draw selected reroute points with an outline
                                    if (_selectedReroutes.Contains(rerouteRef))
                                    {
                                        GUI.color = Preferences.GetSettings().HighlightColor;
                                        GUI.DrawTexture(rect, portStyle.normal.background);
                                    }

                                    GUI.color = portColor;
                                    GUI.DrawTexture(rect, portStyle.active.background);
                                    if (rect.Overlaps(_selectionBox))
                                        selection.Add(rerouteRef);
                                    if (rect.Contains(mousePos))
                                        _hoveredReroute = rerouteRef;
                                }
                            }
                        }
                    }

                    DrawPortHandle(portRectInWindowSpace, backgroundColor, portColor, portStyle.normal.background, portStyle.active.background);
                }
            }

            GUI.color = col;
            if (Event.current.type != EventType.Layout && CurrentActivity == NodeActivity.BoxSelect)
                _selectedReroutes = selection;
        }

        private Rect DrawArrow(IO io, Vector2 point, Color color)
        {
            bool isInput = io == IO.Input;
            Texture icon = isInput ? EditorIcons.TriangleLeft.Active : EditorIcons.TriangleRight.Active;

            Rect rect = default;
            rect.size = new Vector2(ArrowWidth, ArrowWidth) * 2f;
            rect.center = point + Vector2.right * ArrowWidth * 0.5f * (isInput ? -1 : 1) - Vector2.up;

            rect = GridToWindowRect(rect);

            var previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, icon);
            GUI.color = previousColor;

            return rect;
        }

        private void DrawNodes()
        {
            Event e = Event.current;

            BeginZoomed(position, Zoom, TopPadding);

            Vector2 mousePos = e.mousePosition;

            if (e.type != EventType.Layout)
            {
                _hoveredNode = null;
            }

            var preSelection = new List<Object>(_preBoxSelection);

            // Selection box stuff
            Vector2 boxStartPos = GridToWindowPositionNoClipped(_dragBoxStart);
            Vector2 boxSize = mousePos - boxStartPos;
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

            Rect selectionBox = new Rect(boxStartPos, boxSize);

            //Save guiColor so we can revert it
            Color guiColor = GUI.color;

            List<Port> removeEntries = new List<Port>();

            if (e.type == EventType.Layout)
            {
                _culledEditors.Clear();

                _stickyEditors.Clear();

                foreach (Object o in Selection.objects)
                {
                    if (o is NodeEditor editor)
                    {
                        _stickyEditors.Add(editor);
                        foreach (var kvp in editor.Ports)
                        {
                            var connection = kvp.Value.Connection;
                            if (connection is not null)
                                _stickyEditors.Add(connection);
                        }

                        foreach (var (otherNode, otherEditor) in _nodesToEditor)
                        {
                            foreach (var (path, port) in otherEditor.Ports)
                            {
                                if (port.Connection == editor)
                                {
                                    _stickyEditors.Add(otherEditor);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            foreach (var (value, editor) in _nodesToEditor)
            {
                if (Graph.Nodes.Contains(value))
                    continue;

                foreach (var value2 in _nodesToEditor.Keys.ToArray())
                {
                    if (Graph.Nodes.Contains(value2) == false)
                    {
                        _nodesToEditor.Remove(value2, out var v);
                        v?.ObjectTree.Dispose();
                    }
                }
                break;
            }

            Undo.RecordObject(Graph, $"Changed {Graph.name}");
            EditorGUI.BeginChangeCheck();

            foreach (var (node, editor) in _nodesToEditor)
            {
                if (_stickyEditors.Contains(editor))
                    continue;

                DrawNodeEditor(e, editor, false, removeEntries, guiColor, mousePos, selectionBox, preSelection);
            }

            if (_stickyEditors.Count > 0)
            {
                var prevColor = GUI.color;
                GUI.color = new Color(0, 0, 0, 0.5f);
                GUI.DrawTexture(Rect.MinMaxRect(-10000, -10000, 10000, 10000), Texture2D.whiteTexture);
                GUI.color = prevColor;
            }

            foreach (var editor in _stickyEditors)
            {
                DrawNodeEditor(e, editor, true, removeEntries, guiColor, mousePos, selectionBox, preSelection);
            }

            if (EditorGUI.EndChangeCheck())
                Undo.FlushUndoRecordObjects();

            if (e.type == EventType.Repaint && CurrentActivity == NodeActivity.BoxSelect)
                Selection.objects = preSelection.ToArray();
            EndZoomed(position, Zoom, TopPadding);
        }

        private Vector2 GetStickyWindowPosition(NodeEditor nodeEditor)
        {
            Vector2 nodePos = GridToWindowPositionNoClipped(nodeEditor.Value.Position);

            if (nodePos.x < 0)
                nodePos.x = 0;
            if (nodePos.y < 0)
                nodePos.y = 0;

            if (nodeEditor.CachedSize != default)
            {
                Vector2 size = nodeEditor.CachedSize;
                Vector2 max = nodePos + size;
                if (max.x > this.position.size.x*_zoom)
                    nodePos.x = this.position.size.x*_zoom - size.x;
                if (max.y > this.position.size.y*_zoom)
                    nodePos.y = this.position.size.y*_zoom - size.y;
            }
            return nodePos;
        }

        private Vector2 GetStickyGridPosition(NodeEditor nodeEditor)
        {
            return GetStickyWindowPosition(nodeEditor) - (position.size * (0.5f * Zoom) + PanOffset);
        }

        private void DrawNodeEditor(Event e, NodeEditor nodeEditor, bool sticky, List<Port> removeEntries,
            Color guiColor, Vector2 mousePos, Rect selectionBox, List<Object> preSelection)
        {
            // Culling
            if (e.type == EventType.Layout)
            {
                // Cull unselected nodes outside view
                if (!Selection.Contains(nodeEditor) && sticky == false && ShouldBeCulled(nodeEditor))
                {
                    _culledEditors.Add(nodeEditor);
                    return;
                }
            }
            else if (_culledEditors.Contains(nodeEditor))
                return;

            if (e.type == EventType.Repaint)
            {
                removeEntries.Clear();
                foreach (var (_, port) in nodeEditor.Ports)
                    port.CachedRect = default;
            }

            // Set default label width. This is potentially overridden in OnBodyGUI
            EditorGUIUtility.labelWidth = 84;

            //Get node position
            Vector2 nodePos = GridToWindowPositionNoClipped(nodeEditor.Value.Position);
            if (sticky)
            {
                if (nodePos.x < 0)
                    nodePos.x = 0;
                if (nodePos.y < 0)
                    nodePos.y = 0;
                if (nodeEditor.CachedSize != default)
                {
                    Vector2 size = nodeEditor.CachedSize;
                    Vector2 max = nodePos + size;
                    if (max.x > this.position.size.x*_zoom)
                        nodePos.x = this.position.size.x*_zoom - size.x;
                    if (max.y > this.position.size.y*_zoom)
                        nodePos.y = this.position.size.y*_zoom - size.y;
                }
            }

            GUILayout.BeginArea(new Rect(nodePos, new Vector2(nodeEditor.GetWidth(), 4000)));

            bool highlighted = Selection.objects.Contains(nodeEditor);
            highlighted |= _draggedPort?.CanConnectTo(nodeEditor.Value.GetType()) == true;

            GUIStyle verticalStyle;
            if (highlighted)
            {
                GUIStyle style = new GUIStyle(nodeEditor.GetBodyStyle());
                verticalStyle = new GUIStyle(nodeEditor.GetBodyHighlightStyle());
                verticalStyle.padding = style.padding;
                style.padding = new RectOffset();
                GUI.color = nodeEditor.GetTint();
                GUILayout.BeginVertical(style);
                GUI.color = Preferences.GetSettings().HighlightColor;
            }
            else
            {
                verticalStyle = nodeEditor.GetBodyStyle();
                GUI.color = nodeEditor.GetTint();
            }
            GUILayout.BeginVertical(verticalStyle);

            GUI.color = guiColor;
            EditorGUI.BeginChangeCheck();

            //Draw node contents
            nodeEditor.OnHeaderGUI();
            nodeEditor.OnBodyGUI();

            //If user changed a value, notify other scripts through onUpdateNode
            if (EditorGUI.EndChangeCheck())
                nodeEditor.SerializedObject.ApplyModifiedProperties();

            GUILayout.EndVertical();

            //Cache data about the node for next frame
            if (e.type == EventType.Repaint)
            {
                Vector2 size = GUILayoutUtility.GetLastRect().size;
                nodeEditor.CachedSize = size;

                foreach (var (_, port) in nodeEditor.Ports)
                {
                    Vector2 portHandlePos;
                    if (port.Direction == IO.Output)
                        portHandlePos.x = size.x;
                    else
                        portHandlePos.x = 0;
                    portHandlePos.y = port.CachedHeight;
                    if (_stickyEditors.Contains(nodeEditor))
                        portHandlePos += GetStickyGridPosition(nodeEditor);
                    else
                        portHandlePos += nodeEditor.Value.Position;
                    Rect rect = new Rect(portHandlePos.x - 8, portHandlePos.y - 8, 16, 16);
                    port.CachedRect = rect;
                }
            }

            if (highlighted) GUILayout.EndVertical();

            if (e.type != EventType.Layout)
            {
                //Check if we are hovering this node
                Vector2 nodeSize = GUILayoutUtility.GetLastRect().size;
                Rect windowRect = new Rect(nodePos, nodeSize);
                if (windowRect.Contains(mousePos))
                    _hoveredNode = nodeEditor;

                //If dragging a selection box, add nodes inside to selection
                if (e.type == EventType.Repaint && CurrentActivity == NodeActivity.BoxSelect && windowRect.Overlaps(selectionBox))
                {
                    preSelection.Add(nodeEditor);
                }
            }

            GUILayout.EndArea();
        }

        private bool ShouldBeCulled(NodeEditor nodeEditor)
        {
            if (_firstRun)
                return false;

            Vector2 nodePos = GridToWindowPositionNoClipped(nodeEditor.Value.Position);
            if (nodePos.x / _zoom > position.width) return true; // Right
            else if (nodePos.y / _zoom > position.height) return true; // Bottom
            else if (nodeEditor.CachedSize != default)
            {
                Vector2 size = nodeEditor.CachedSize;
                Vector2 max = nodePos + size;
                if (max.x < 0 || max.y < 0)
                    return true;
            }

            return false;
        }

        private bool ShouldBeCulled(Rect rect)
        {
            if (_firstRun)
                return false;

            rect = GridToWindowRect(rect);
            var screenRect = new Rect(default, position.size);
            return rect.Overlaps(screenRect) == false;
        }

        private void DrawTooltip()
        {
            if (!Preferences.GetSettings().PortTooltips)
                return;

            string? tooltip = null;
            if (_hoveredPort != null)
            {
                tooltip = GetPortTooltip(_hoveredPort);
            }
            else if (_hoveredNode != null && _hoveredNode != null && IsHoveringTitle(_hoveredNode))
            {
                tooltip = _hoveredNode.GetHeaderTooltip();
            }

            if (string.IsNullOrEmpty(tooltip)) return;
            GUIContent content = new GUIContent(tooltip);
            Vector2 size = Resources.Styles.Tooltip.CalcSize(content);
            size.x += 8;
            Rect rect = new Rect(Event.current.mousePosition - (size*new Vector2(0.5f, 1f)), size);
            EditorGUI.LabelField(rect, content, Resources.Styles.Tooltip);
            Repaint();
        }

        /// <summary>
        /// Draw the port
        /// </summary>
        /// <param name="rect">position and size</param>
        /// <param name="backgroundColor">color for background texture of the port. Normaly used to Border</param>
        /// <param name="typeColor"></param>
        /// <param name="border">texture for border of the dot port</param>
        /// <param name="dot">texture for the dot port</param>
        private void DrawPortHandle(Rect rect, Color backgroundColor, Color typeColor, Texture2D border, Texture2D dot)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Color col = GUI.color;
            GUI.color = backgroundColor;
            GUI.DrawTexture(rect, border);
            GUI.color = typeColor;
            GUI.DrawTexture(rect, dot);
            GUI.color = col;
        }
    }
}
