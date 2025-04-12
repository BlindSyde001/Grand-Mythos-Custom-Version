using System;
using System.Collections.Generic;
using Screenplay.Component;
using Source.Screenplay.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using YNode;
using YNode.Editor;
using Screenplay.Nodes;
using Action = Screenplay.Nodes.Action;
using Event = Screenplay.Nodes.Event;

namespace Screenplay.Editor
{
    public class ScreenplayEditor : CustomGraphWindow<ScreenplayGraph>
    {
        private List<INodeValue> _previewChain = new();
        private System.Action? _disposeCallbacks;
        private Previewer? _previewer;
        private bool _previewEnabled;
        private PreviewFlags _previewFlags = PreviewFlags.Loop | PreviewFlags.SelectedChain;
        private bool _hasFocus;

        public IReadOnlyList<INodeValue> PreviewChain => _previewChain;

        protected override void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnGUI();
            if (EditorGUI.EndChangeCheck())
            {
                Rollback();
                TryPreview();
                RecalculateReachable();
            }
        }

        protected override void Load()
        {
            base.Load();
            RecalculateReachable();
        }

        protected override void OnGUIOverlay()
        {
            base.OnGUIOverlay();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.FlexibleSpace();

                var previousColor = GUI.backgroundColor;
                if (_previewEnabled)
                    GUI.backgroundColor *= new Color(0.75f, 0.75f, 1.0f, 1f);
                _previewEnabled = GUILayout.Button("Preview", EditorStyles.toolbarButton) ? !_previewEnabled : _previewEnabled;
                GUI.backgroundColor = previousColor;
                _previewFlags = (PreviewFlags)EditorGUILayout.EnumFlagsField(_previewFlags, EditorStyles.toolbarPopup);
            }
            EditorGUILayout.EndHorizontal();

            foreach (var value in _previewChain)
                TryGetEditorFromValue(value)!.InPreviewPath = false;

            TryPreview();

            var dispatcher = FindObjectOfType<ScreenplayDispatcher>();
            if (dispatcher == null)
                EditorGUILayout.HelpBox("Add a ScreenplayDispatcher to the scene to run this screenplay", MessageType.Warning);
            else if (dispatcher.Screenplay != Graph)
                EditorGUILayout.HelpBox("The existing ScreenplayDispatcher doesn't run with this screenplay, you might want to assign it to this screenplay", MessageType.Warning);
        }

        [ThreadStatic]
        private static HashSet<IAction>? _isNodeReachableVisitation;
        void RecalculateReachable()
        {
            _isNodeReachableVisitation ??= new();
            _isNodeReachableVisitation.Clear();
            foreach (var node in Graph.Nodes)
            {
                if (TryGetEditorFromValue(node) is {} editor)
                    editor.Reachable = false;
            }


            foreach (var node in Graph.Nodes)
            {
                if (node is Event e && e.Action is not null)
                    TraverseTree(e.Action);
            }

            void TraverseTree(IAction branch)
            {
                if (_isNodeReachableVisitation!.Add(branch) == false)
                    return;

                if (TryGetEditorFromValue(branch) is {} editor)
                    editor.Reachable = true;

                foreach (IAction otherActions in branch.Followup())
                    TraverseTree(otherActions);
            }
        }

        void TryPreview()
        {
            var previousSelection = _previewChain.Count > 0 ? _previewChain[^1] : null;
            _previewChain.Clear();
            if (_previewEnabled
                && (_previewFlags.Contains(PreviewFlags.Unfocused) || _hasFocus)
                && (_previewFlags.Contains(PreviewFlags.InPlayMode) || EditorApplication.isPlaying == false))
            {
                if (Selection.activeObject is YNode.Editor.NodeEditor selectedNode && selectedNode.Graph == Graph && selectedNode.Value is IPreviewable selectedPreviewable)
                {
                    if (_previewFlags.Contains(PreviewFlags.SelectedChain) && selectedPreviewable is IAction selectedAction)
                    {
                        Graph.IsNodeReachable(selectedAction, _previewChain);
                    }
                    else
                    {
                        _previewChain.Add(selectedPreviewable);
                    }
                }
            }

            foreach (var value in _previewChain)
                TryGetEditorFromValue(value)!.InPreviewPath = true;

            var currentSelection = _previewChain.Count > 0 ? _previewChain[^1] : null;
            if (currentSelection is null)
            {
                Rollback();
            }
            else if (_previewer is null || previousSelection != currentSelection) // Selection changed
            {
                Rollback();
                _previewer = new Previewer(_previewFlags.Contains(PreviewFlags.Loop), Graph.DialogUIPrefab, Graph);
                for (int i = 0; i < _previewChain.Count; i++)
                {
                    if (_previewChain[i] is IPreviewable previewable)
                        previewable.SetupPreview(_previewer, i+1 != _previewChain.Count);
                }
            }
        }

        void OnSceneGUI(SceneView view)
        {
            bool rebuildPreview = false;
            foreach (var o in Selection.objects)
            {
                if (o is YNode.Editor.NodeEditor nodeEditor && nodeEditor.Value is INodeWithSceneGizmos sceneGizmos)
                    sceneGizmos.DrawGizmos(ref rebuildPreview);
            }

            if (rebuildPreview)
            {
                Rollback();
                TryPreview();
            }
        }

        NodeEditor? TryGetEditorFromValue(INodeValue value)
        {
            return NodesToEditor[value] as NodeEditor;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;

            AssemblyReloadEvents.AssemblyReloadCallback assReloadCallback = Rollback;
            Action<PlayModeStateChange> pmsChanged = change => Rollback();
            EditorSceneManager.SceneSavingCallback ssCallback = (scene, path) => Rollback();
            EditorSceneManager.SceneClosingCallback scCallback = (scene, path) => Rollback();

            AssemblyReloadEvents.beforeAssemblyReload += assReloadCallback;
            EditorApplication.playModeStateChanged += pmsChanged;
            EditorSceneManager.sceneSaving += ssCallback;
            EditorSceneManager.sceneClosing += scCallback;

            _disposeCallbacks += () =>
            {
                AssemblyReloadEvents.beforeAssemblyReload -= assReloadCallback;
                EditorApplication.playModeStateChanged -= pmsChanged;
                EditorSceneManager.sceneSaving -= ssCallback;
                EditorSceneManager.sceneClosing -= scCallback;
            };
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _disposeCallbacks?.Invoke();
            _disposeCallbacks = null;
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        public override void OnFocus()
        {
            base.OnFocus();
            _hasFocus = true;
        }

        public override void OnLostFocus()
        {
            base.OnLostFocus();
            if (_previewFlags.Contains(PreviewFlags.Unfocused) == false)
                Rollback();
            _hasFocus = false;
        }

        public override string GetNodeMenuName(System.Type type)
        {
            // ReSharper disable once RedundantNameQualifier
            if (typeof(ScreenplayNode).IsAssignableFrom(type) || type == typeof(Notes))
            {
                var str = base.GetNodeMenuName(type);
                string comparison = typeof(Action).Namespace!.Replace('.', '/') + "/";
                if (str.StartsWith(comparison))
                    return str[comparison.Length..];
                return str;
            }

            return "";
        }



        public void Rollback()
        {
            _previewer?.Dispose();
            _previewer = null;
        }

        [Flags]
        public enum PreviewFlags
        {
            [Tooltip("Preview the whole chain up to the selected node")]
            SelectedChain =  0b0001,
            [Tooltip("Restart the previewed node as soon as it finished the preview")]
            Loop =           0b0010,
            [Tooltip("Play the preview even when the window is not in focus, do know that making changes to things that are being played will lead to undefined behavior")]
            Unfocused =      0b0100,
            [Tooltip("Play the preview even when a game is currently running")]
            InPlayMode =     0b1000,
        }
    }

    public static class EnumExtension
    {
        public static bool Contains(this ScreenplayEditor.PreviewFlags a, ScreenplayEditor.PreviewFlags b) =>
            (a & b) == b;
    }
}
