using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Object = UnityEngine.Object;

namespace YNode.Editor
{
    public abstract partial class GraphWindow : EditorWindow
    {
        public NodeGraph Graph = null!;

        private Func<bool>? _isDocked;
        private Vector2 _panOffset;
        private float _zoom = 1;

        public abstract string PreferenceKey { get; }

        private Func<bool> IsDocked
        {
            get => _isDocked ??= this.GetIsDockedDelegate();
        }

        public Vector2 PanOffset
        {
            get { return _panOffset; }
            set
            {
                _panOffset = value;
                Repaint();
            }
        }

        public float Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = Mathf.Clamp(value, Preferences.GetSettings().MinZoom,
                    Preferences.GetSettings().MaxZoom);
                Repaint();
            }
        }

        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;
        }

        /// <summary> Handle Selection Change events</summary>
        private static void OnSelectionChanged()
        {
            if (Selection.activeObject is NodeGraph nodeGraph && !AssetDatabase.Contains(nodeGraph))
            {
                if (Preferences.GetSettings().OpenOnCreate)
                    Open(nodeGraph);
            }
        }

        public void Save()
        {
            if (AssetDatabase.Contains(Graph))
            {
                EditorUtility.SetDirty(Graph);
                AssetDatabase.SaveAssetIfDirty(Graph);
            }
            else
                SaveAs();
        }

        public void SaveAs()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save NodeGraph", "NewNodeGraph", "asset", "");
            if (string.IsNullOrEmpty(path))
                return;

            NodeGraph existingGraph = AssetDatabase.LoadAssetAtPath<NodeGraph>(path);
            if (existingGraph != null) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(Graph, path);
            EditorUtility.SetDirty(Graph);
            AssetDatabase.SaveAssetIfDirty(Graph);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 WindowToGridPosition(Vector2 windowPosition)
        {
            return (windowPosition - (position.size * 0.5f) - (PanOffset / Zoom)) * Zoom;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 GridToWindowPosition(Vector2 gridPosition)
        {
            return (position.size * 0.5f) + (PanOffset / Zoom) + (gridPosition / Zoom);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rect GridToWindowRectNoClipped(Rect gridRect)
        {
            gridRect.position = GridToWindowPositionNoClipped(gridRect.position);
            return gridRect;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rect GridToWindowRect(Rect gridRect)
        {
            gridRect.position = GridToWindowPosition(gridRect.position);
            gridRect.size /= Zoom;
            return gridRect;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 GridToWindowPositionNoClipped(Vector2 gridPosition)
        {
            Vector2 center = position.size * (0.5f * Zoom) + PanOffset + gridPosition;
            // UI Sharpness complete fix - Round final offset not panOffset
            center.x = Mathf.Round(center.x);
            center.y = Mathf.Round(center.y);
            return center;
        }

        public void SelectNode(NodeEditor nodeEditor, bool add)
        {
            if (add)
            {
                Selection.objects = Selection.objects.Append(nodeEditor).ToArray();
            }
            else
            {
                Selection.objects = new Object[] { nodeEditor };
            }
        }

        public void DeselectNode(NodeEditor nodeEditor)
        {
            Selection.objects = Selection.objects.Where(x => x != nodeEditor).ToArray();
        }

        [OnOpenAsset(0)]
        public static bool OnOpen(int instanceID, int line)
        {
            if (EditorUtility.InstanceIDToObject(instanceID) is NodeGraph nodeGraph)
            {
                Open(nodeGraph);
                return true;
            }

            return false;
        }

        /// <summary>Open the provided graph in the NodeEditor</summary>
        public static GraphWindow Open(NodeGraph graph)
        {
            var windowType = Utilities.GetCustomEditor(graph.GetType(), typeof(CustomGraphWindow<>), typeof(GraphWindow));
            var window = (GraphWindow)CreateInstance(windowType);
            window.titleContent.image = EditorGUIUtility.GetIconForObject(graph) ?? EditorGUIUtility.ObjectContent(null, graph.GetType())?.image;
            window.titleContent.text = graph.name;
            window.wantsMouseMove = true;
            window.Graph = graph;
            window.Show();
            window.Focus();
            return window;
        }

        /// <summary> Repaint all open NodeEditorWindows. </summary>
        public static void RepaintAll()
        {
            foreach (var window in UnityEngine.Resources.FindObjectsOfTypeAll<GraphWindow>())
            {
                window.Repaint();
            }
        }
    }
}
