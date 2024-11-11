using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Screenplay.Nodes;
using UnityEngine;
using YNode;
using Event = Screenplay.Nodes.Event;

namespace Screenplay
{
    public static class ScreenplayGizmos
    {
        private static Dictionary<INodeValue, Vector3> s_anchor = new();
        private static HashSet<INodeValue> s_visited = new();
        private static List<GenericSceneObjectReference> s_workingList = new();
        private static List<Vector3> s_lineForPathList = new();
        private static List<Event> s_events = new();
        private static List<IAction> s_stack = new();

        public static void Draw(ScreenplayGraph screenplay)
        {
            s_events.Clear();
            s_anchor.Clear();
            s_workingList.Clear();
            s_lineForPathList.Clear();

            foreach (var node in screenplay.Nodes)
            {
                if (node is IReferenceContainer refContainer)
                {
                    refContainer.CollectReferences(s_workingList);
                    if (s_workingList.Count > 0)
                    {
                        if (ParseWorkingListAndGetAnchor(out Vector3 anchor))
                            s_anchor[node] = anchor;

                        s_workingList.Clear();
                    }
                }

                if (node is Event @event)
                {
                    s_events.Add(@event);
                }
            }

            for (int i = 0; i < s_events.Count; i++)
            {
                s_lineForPathList.Clear();
                s_visited.Clear();
                s_stack.Clear();

                var e = s_events[i];
                float h = i / (float)s_events.Count;
                var color = Color.HSVToRGB(h, 0.5f, 0.5f);

                #if UNITY_EDITOR
                UnityEditor.Handles.color = color;
                #endif

                Vector3? root = null;
                if (e.TriggerSource is {} trigger && s_anchor.TryGetValue(trigger, out var anchor))
                {
                    if (e.Prerequisite is {} prereq && s_anchor.TryGetValue(prereq, out var anchorPrereq))
                    {
                        root = (anchorPrereq + anchor) / 2;
                        Gizmos.color = color * new Color(1,1,1,0.25f);
                        Gizmos.DrawLine(anchorPrereq, root.Value);
                        Gizmos.DrawLine(anchor, root.Value);
                    }
                    else
                    {
                        root = anchor;
                    }
                }
                else if (e.Prerequisite is {} prereq && s_anchor.TryGetValue(prereq, out anchor))
                    root = anchor;

                if (root is { } r)
                {
                    #if UNITY_EDITOR
                    UnityEditor.Handles.Label(r, e.Name);
                    #endif
                }

                if (e.Action is { } act)
                {
                    RecursivePathBuilder(act, root, true);
                }

                if (s_lineForPathList.Count > 0)
                {
                    Gizmos.color = color;
                    Gizmos.DrawLineList(AsSpan(s_lineForPathList));
                }
            }

            static void RecursivePathBuilder(IAction action, Vector3? root, bool label)
            {
                s_stack.Add(action);
                try
                {
                    bool hasFollowup = false;
                    if (s_visited.Add(action))
                    {
                        foreach (var next in action.Followup())
                        {
                            hasFollowup = true;
                            RecursivePathBuilder(next, root, label);
                        }
                    }

                    if (hasFollowup == false)
                    {
                        // End of the path, draw along the path
                        Vector3? lastP = root;
                        for (int i = 0, lastIndex = 0; i < s_stack.Count; i++)
                        {
                            if (s_anchor.TryGetValue(s_stack[i], out var thisP) == false)
                            {
                                if (i == s_stack.Count - 1 && lastP is not null) // We're on the last one, try to assign as best we can with what we have if any
                                    thisP = lastP.Value;
                                else
                                    continue;
                            }

                            if (lastP is not null) // Here the values could be root if any, or the last real anchor
                            {
                                s_lineForPathList.Add(lastP.Value);
                                s_lineForPathList.Add(thisP);
                                #if UNITY_EDITOR
                                if (label && lastP.Value != thisP)
                                {
                                    // Find position for intermediary nodes which don't have any position

                                    {
                                        UnityEditor.Handles.Label(thisP, s_stack[i].GetType().Name);
                                        for (int j = lastIndex + 1; j < i; j++)
                                        {
                                            UnityEditor.Handles.Label(Vector3.Lerp(lastP.Value, thisP, (float)(j - lastIndex) / (i - lastIndex)), s_stack[j].GetType().Name);
                                        }
                                    }
                                }
                                #endif
                            }

                            lastP = thisP;
                            lastIndex = i;
                        }
                    }
                }
                finally
                {
                    s_stack.RemoveAt(s_stack.Count-1);
                }
            }
        }

        private static bool ParseWorkingListAndGetAnchor(out Vector3 anchor)
        {
            Span<Vector3> buffer = stackalloc Vector3[s_workingList.Count];
            int found = 0;
            foreach (var sRef in s_workingList)
            {
                if (sRef.TryGet(out var obj, out _) == false)
                    continue;

                Vector3 posA;
                if (obj is UnityEngine.Component c)
                    posA = c.transform.position;
                else
                    posA = ((GameObject)obj).transform.position;
                buffer[found++] = posA;

                #if UNITY_EDITOR
                var tex = UnityEditor.EditorGUIUtility.GetIconForObject(obj) ?? UnityEditor.EditorGUIUtility.ObjectContent(obj, obj.GetType())?.image;
                if (tex != null)
                {
                    Camera sceneCamera = UnityEditor.SceneView.lastActiveSceneView.camera;
                    Vector3 pos = sceneCamera.WorldToScreenPoint(posA);
                    Rect rect = new Rect(pos, new Vector2(10, 10));
                    Gizmos.DrawGUITexture(rect, tex);
                }
                #endif
            }

            if (found == 1)
            {
                anchor = buffer[0];
                return true;
            }
            else if (found > 0)
            {
                anchor = Vector3.zero;
                buffer = buffer[..found];
                foreach (var pos in buffer)
                    anchor += pos;

                anchor /= found;
                return true;
            }

            anchor = default;
            return false;
        }

        public static Span<T> AsSpan<T>(List<T>? list)
        {
            if (list == null)
                return default;

            var box = new ListCastHelper { List = list }.StrongBox;
            return new Span<T>((T[])box!.Value, 0, list.Count);
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct ListCastHelper
        {
            [FieldOffset(0)]
            public StrongBox<Array> StrongBox;

            [FieldOffset(0)]
            public object List;
        }
    }
}
