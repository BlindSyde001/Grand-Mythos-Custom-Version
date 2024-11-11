using System;
using System.Linq;
using Screenplay.Nodes;
using Screenplay.Nodes.TrackItems;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Event = UnityEngine.Event;

namespace Screenplay.Editor
{
    public class TrackEditor : OdinValueDrawer<Track>
    {
        private const string _tooltip = "Hold the Control key while dragging to prevent rounding down to the closest frame\nMiddle or left mouse button to pan the view\nF to recenter the view";
        private static Color s_textColor = Color.black;
        private static Color s_markerColor = new Color32(100,100,200,127);
        private static Color s_markerTextColor = new Color32(200,200,200,255);
        private static Color s_background = new Color(0,0,0,0.25f);
        private static Color s_timeTickColor = new Color(0,0,0,0.25f);
        private static Color s_itemColor = new Color32(70,96,124,255);
        private static Color s_itemLabelColor = new Color32(255,255,255,255);
        private static Color s_playHeadColor = new Color32(225,150,100,255);
        private static GUIStyle? s_leftStyle, s_middleStyle, s_rightStyle, s_itemLabel;
        private static GUIContent s_tempContent = new GUIContent();

        private float _start, _end;
        private ITrackItem? _itemHover, _itemHeld;
        private int? _markerHover, _markerHeld;
        private DragAction _dragAction;
        private Rect _itemHoverRect;
        private float _fractionalDrag;
        private Vector2 _lastPos;
        private float _playHead
        {
            get => ValueEntry.SmartValue.DebugPlayHead;
            set => ValueEntry.SmartValue.DebugPlayHead = value;
        }
        private float _timeUnderCursor;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var track = ValueEntry.SmartValue;

            // Draw info tooltip
            EditorGUILayout.BeginHorizontal();
            s_tempContent.image = EditorIcons.Info.Active;
            s_tempContent.tooltip = _tooltip;
            s_tempContent.text = null;
            EditorGUILayout.LabelField(s_tempContent);
            s_tempContent.image = null;
            s_tempContent.tooltip = null;

            GUILayout.FlexibleSpace();
            track.DebugScrub = (Track.PreviewMode)EditorGUILayout.EnumPopup(track.DebugScrub);
            EditorGUILayout.EndHorizontal();

            CallNextDrawer(label);

            EditorGUILayout.Space();

            if (_end == _start)
            {
                _start = 0f;
                _end = track.Duration();
            }

            var previousColor = GUI.color;
            var viewerRect = DrawTimeline(track, _start, _end);
            GUI.color = previousColor;

            var e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:
                    if (e.keyCode == KeyCode.F)
                    {
                        _start = 0f;
                        _end = track.Duration();
                        e.Use();
                    }
                    break;
                case EventType.MouseDown:
                    if (Property.Tree.WeakTargets[0] is UnityEngine.Object obj && Selection.Contains(obj) == false)
                        Selection.objects = Selection.objects.Append(obj).ToArray();

                    if (e.button == 0 && (_itemHover != null || _markerHover != null || viewerRect.Contains(e.mousePosition)))
                    {
                        DragAction newDragAction;
                        float newFractionalDrag;
                        if (_markerHover is {} markerIndex)
                        {
                            _markerHeld = markerIndex;
                            newDragAction = DragAction.MarkerDrag;
                            newFractionalDrag = track.Markers[markerIndex].Time;
                        }
                        else if (_itemHover == null)
                        {
                            newDragAction = DragAction.PlayheadDrag;
                            newFractionalDrag = _playHead;
                        }
                        else
                        {
                            _itemHeld = _itemHover;
                            DraggableArea(_itemHoverRect, out var leftSide, out var rightSide);
                            if (leftSide.Contains(e.mousePosition))
                            {
                                newDragAction = DragAction.ItemStretchLeft;
                                newFractionalDrag = _itemHeld.Start;
                            }
                            else if (rightSide.Contains(e.mousePosition))
                            {
                                newDragAction = DragAction.ItemStretchRight;
                                newFractionalDrag = _itemHeld.Start + _itemHeld.Duration;
                            }
                            else
                            {
                                newDragAction = DragAction.ItemMove;
                                newFractionalDrag = _itemHeld!.Start;
                            }
                        }

                        _dragAction = newDragAction;
                        _fractionalDrag = newFractionalDrag;
                        _lastPos = e.mousePosition;

                        e.Use();
                    }
                    else if (e.button != 0 && viewerRect.Contains(e.mousePosition))
                    {
                        _lastPos = e.mousePosition;
                        _dragAction = DragAction.ViewPan;
                        e.Use();
                    }

                    break;
                case EventType.MouseUp:
                    if (_dragAction != DragAction.None)
                    {
                        _dragAction = DragAction.None;
                        _itemHeld = null;
                        _markerHeld = null;
                        e.Use();
                    }

                    break;
                case EventType.MouseDrag:
                    if (e.button == 0 && _dragAction != DragAction.None)
                    {
                        float delta = (e.mousePosition.x - _lastPos.x) / viewerRect.width * (_end - _start);
                        _fractionalDrag += delta;
                        bool roundOff = e.control == false;
                        // ReSharper disable CompareOfFloatsByEqualityOperator
                        switch (_dragAction)
                        {
                            case DragAction.ItemMove:
                                var newValue = RoundOff(_fractionalDrag, roundOff);
                                GUI.changed |= _itemHeld!.Start != newValue;
                                _itemHeld.Start = newValue;
                                break;
                            case DragAction.ItemStretchLeft:
                                var prev = _itemHeld!.Start;
                                _itemHeld.Start = RoundOff(_fractionalDrag, roundOff);
                                _itemHeld.Duration += prev - _itemHeld.Start;
                                GUI.changed |= _itemHeld!.Start != prev;
                                break;
                            case DragAction.ItemStretchRight:
                                newValue = RoundOff(_fractionalDrag, roundOff) - _itemHeld!.Start;
                                GUI.changed |= _itemHeld!.Duration != newValue;
                                _itemHeld!.Duration = newValue;
                                break;
                            case DragAction.PlayheadDrag:
                                _playHead = RoundOff(_timeUnderCursor, roundOff);
                                break;
                            case DragAction.MarkerDrag:
                                newValue = RoundOff(_timeUnderCursor, roundOff);
                                GUI.changed |= newValue != track.Markers[_markerHeld!.Value].Time;
                                track.Markers[_markerHeld!.Value].Time = newValue;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        // ReSharper restore CompareOfFloatsByEqualityOperator
                        static float RoundOff(float a, bool enable) => enable ? (float)(Math.Round(a * 60.0d) / 60.0d) : a;

                        _lastPos = e.mousePosition;
                        e.Use();
                    }
                    else if (_dragAction == DragAction.ViewPan)
                    {
                        float delta = (e.mousePosition.x - _lastPos.x) / viewerRect.width * (_end - _start);
                        _start -= delta;
                        _end -= delta;
                        _lastPos = e.mousePosition;
                        e.Use();
                    }

                    break;
            }
            if (e.isScrollWheel && viewerRect.Contains(e.mousePosition) && Property.Tree.WeakTargets[0] is UnityEngine.Object obj2 && Selection.Contains(obj2))
            {
                float leftRightRatio = (e.mousePosition.x - viewerRect.x) / viewerRect.width;
                float value = e.delta.y * 0.1f;
                _start -= value * leftRightRatio;
                _end += value * (1f - leftRightRatio);
                _start += e.delta.x;
                _end += e.delta.x;
                if (_end < _start)
                    (_start, _end) = (_end, _start);

                e.Use();
            }
        }

        Rect DrawTimeline(Track Value, float start, float end)
        {
            var e = Event.current;

            s_leftStyle ??= new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleLeft };
            s_middleStyle ??= new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleCenter };
            s_rightStyle ??= new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleRight };
            const int divisionPerSeconds = 4;

            Span<int> itemToLane = stackalloc int[Value.Items.Length];
            int lanes = 0;
            for (int i = 0; i < Value.Items.Length; i++) // Place items that wouldn't overlap on the same lane
            {
                var item = Value.Items[i];
                if (item == null)
                    continue;

                var thisSpan = item.Timespan;
                int lane;
                for (lane = 0; lane < i; lane++) // Test each lanes
                {
                    bool collision = false;
                    for (int j = 0; j < i; j++)
                    {
                        if (j == i)
                            continue;
                        if (itemToLane[j] != lane)
                            continue; // This item is not on this lane, exit

                        var otherItem = Value.Items[j];
                        if (otherItem == null)
                            continue;

                        var otherTimespan = otherItem.Timespan;
                        if (thisSpan.start > otherTimespan.end && thisSpan.end > otherTimespan.end
                            || thisSpan.start < otherTimespan.start && thisSpan.end < otherTimespan.start)
                            continue;

                        collision = true;
                        break;
                    }

                    if (collision == false)
                        break;
                }

                itemToLane[i] = lane;
                lanes = lane+1 > lanes ? lane+1 : lanes;
            }

            var view = GUILayoutUtility.GetRect(1, EditorGUIUtility.singleLineHeight * (lanes + 2), GUILayout.ExpandWidth(true));
            var lineRect = view;
            lineRect.height = EditorGUIUtility.singleLineHeight;

            GUI.color = s_background;
            GUI.DrawTexture(view, Texture2D.whiteTexture); // Background

            var viewIterative = view;
            viewIterative.y += lineRect.height;
            viewIterative.height -= lineRect.height;

            _itemHover = null;
            float itemHeight = EditorGUIUtility.singleLineHeight;
            for (int i = 0; i < Value.Items.Length; i++)
            {
                var item = Value.Items[i];
                if (item is null)
                    continue;

                var itemSpan = item.Timespan;
                float itemStart = NormalizeABetweenBC(itemSpan.start, start, end);
                float itemSize = NormalizeABetweenBC(itemSpan.end, start, end) - itemStart;
                if (itemStart < 0)
                {
                    itemSize += itemStart;
                    itemStart = 0;
                }

                var rect = viewIterative;
                rect.y += itemHeight * itemToLane[i];
                rect.height = itemHeight;
                rect.x += itemStart * viewIterative.width;
                rect.width = itemSize * viewIterative.width;
                if (rect.xMax > viewIterative.xMax)
                    rect.xMax = viewIterative.xMax;
                if (rect.xMin > viewIterative.xMax || rect.xMax < viewIterative.xMin)
                    continue;

                GUI.color = s_itemColor;
                if (rect.Contains(e.mousePosition))
                {
                    _itemHover = item;
                    _itemHoverRect = rect;
                }

                bool hoverOrDragged = _itemHeld == item || _itemHover == item;
                if (hoverOrDragged)
                    GUI.color = s_itemColor * 1.2f;

                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = s_itemLabelColor;
                GUI.Label(rect, item.Label, s_middleStyle);

                if (hoverOrDragged)
                {
                    DraggableArea(rect, out var leftSide, out var rightSide);
                    GUI.DrawTexture(leftSide, Texture2D.grayTexture);
                    GUI.DrawTexture(rightSide, Texture2D.grayTexture);
                }
            }

            var markersLane = view;
            markersLane.height = EditorGUIUtility.singleLineHeight;
            markersLane.y += view.height - EditorGUIUtility.singleLineHeight;

            _markerHover = null;
            for (int i = 0; i < Value.Markers.Length; i++)
            {
                var marker = Value.Markers[i];
                float seconds = marker.Time;
                float f = (seconds - start) / (end - start);
                if (f is > 1 or < 0)
                    continue;

                var markerLine = markersLane;
                markerLine.x += markerLine.width * f - 1;
                markerLine.width = 2;
                markerLine.y = view.y;
                markerLine.height = view.height;

                s_tempContent.text = marker.Name;
                var textSize = s_middleStyle.CalcSize(s_tempContent);
                var rectForText = markersLane;
                rectForText.width = textSize.x * 1.1f;
                rectForText.center = new Vector2(markerLine.center.x, markersLane.center.y);

                bool active = rectForText.Contains(e.mousePosition) || _markerHeld == i;

                GUI.color = s_markerColor * (active ? 2 : 1);
                GUI.DrawTexture(markerLine, Texture2D.whiteTexture);

                GUI.color = s_markerTextColor * (active ? 2 : 1);
                GUI.Label(rectForText, s_tempContent, s_middleStyle);

                if (rectForText.Contains(e.mousePosition))
                    _markerHover = i;
            }

            _timeUnderCursor = _start + NormalizeABetweenBC(e.mousePosition.x, lineRect.x, lineRect.xMax) * (_end - _start);
            var playhead = lineRect;
            playhead.width = EditorGUIUtility.singleLineHeight;
            playhead.height = EditorGUIUtility.singleLineHeight;
            playhead.x = lineRect.x + NormalizeABetweenBC(_playHead, start, end) * lineRect.width - playhead.width * 0.5f;
            playhead.y += 3;
            var playheadColor = playhead.Contains(e.mousePosition) || _dragAction == DragAction.PlayheadDrag ? s_playHeadColor * 1.1f : s_playHeadColor;

            var playHeadLine = playhead;
            playHeadLine.y = playHeadLine.center.y;
            playHeadLine.yMax = viewIterative.yMax;
            playHeadLine.width = 2f;
            playHeadLine.center = new Vector2(playhead.center.x, playHeadLine.center.y);
            GUI.color = playheadColor;
            GUI.DrawTexture(playHeadLine, Texture2D.whiteTexture);
            GUI.DrawTexture(playhead, EditorIcons.Marker.Active);

            for (int i = ((int)Math.Floor(start * divisionPerSeconds)) + 1; (float)i / divisionPerSeconds < end; i++)
            {
                float seconds = (float)i / divisionPerSeconds;
                float f = (seconds - start) / (end - start);
                var subdRect = lineRect;
                subdRect.x += subdRect.width * f - 1;
                subdRect.width = 2;
                GUI.color = s_timeTickColor;
                GUI.DrawTexture(subdRect, Texture2D.whiteTexture);

                s_tempContent.text = seconds.ToString("F");
                var size = s_middleStyle.CalcSize(s_tempContent);
                subdRect.x -= size.x;
                subdRect.width = size.x * 2f;
                GUI.color = s_textColor;
                GUI.Label(subdRect, s_tempContent, s_middleStyle);
            }

            var firstTick = lineRect;
            firstTick.width = 2;

            var lastTick = lineRect;
            lastTick.x += lastTick.width - 2;
            lastTick.width = 2;

            GUI.color = s_timeTickColor;
            GUI.DrawTexture(firstTick, Texture2D.whiteTexture);
            GUI.DrawTexture(lastTick, Texture2D.whiteTexture);
            GUI.color = s_textColor;
            GUI.Label(lineRect, start.ToString("F"), s_leftStyle);
            GUI.Label(lineRect, end.ToString("F"), s_rightStyle);

            return view;
        }

        static void DraggableArea(Rect rect, out Rect leftSide, out Rect rightSide)
        {
            leftSide = rect;
            leftSide.width = 10;
            rightSide = rect;
            rightSide.x += rightSide.width - 10;
            rightSide.width = 10;
        }

        static float NormalizeABetweenBC(float t, float start, float end) => (t - start) / (end - start);

        enum DragAction
        {
            None,
            ItemMove,
            ItemStretchLeft,
            ItemStretchRight,
            PlayheadDrag,
            MarkerDrag,
            ViewPan
        }
    }
}
