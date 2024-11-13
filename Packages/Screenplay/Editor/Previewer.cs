using System;
using System.Collections.Generic;
using Screenplay;
using Screenplay.Component;
using UnityEditor;
using UnityEngine;
using Screenplay.Nodes;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Source.Screenplay.Editor
{
    public class Previewer : IPreviewer, IDisposable
    {
        private Stack<System.Action> _rollbacksRegistered = new();
        private List<(object key, IEnumerator<Signal> enumerator)> _players = new();
        private bool _loopPreview;
        private UIBase? _dialogUIComponentPrefab, _dialogUI;

        public ScreenplayGraph Source { get; }
        public bool Loop => _loopPreview;

        public UIBase? GetDialogUI()
        {
            if (_dialogUIComponentPrefab == null)
                return null;

            if (_dialogUI == null)
            {
                _dialogUI = Object.Instantiate(_dialogUIComponentPrefab);
                _rollbacksRegistered.Push(() => Object.DestroyImmediate(_dialogUI.gameObject));
            }

            return _dialogUI;
        }

        public HashSet<IPrerequisite> Visited { get; } = new();

        public Previewer(bool loopPreview, UIBase? dialogUIComponentPrefab, ScreenplayGraph sourceParam)
        {
            _loopPreview = loopPreview;
            _dialogUIComponentPrefab = dialogUIComponentPrefab;
            EditorApplication.update += Update;
            Source = sourceParam;
            UnityEditor.Editor.finishedDefaultHeaderGUI += OnInspectorDrawHeaderGUI;
        }

        public void Dispose()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI -= OnInspectorDrawHeaderGUI;

            for (int i = _players.Count - 1; i >= 0; i--) // Reverse to ensure rollbacks happen leaves first then branches
            {
                var (_, update) = _players[i];
                try
                {
                    update.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            _players.Clear();

            while (_rollbacksRegistered.TryPop(out var action))
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            EditorApplication.update -= Update;
            EditorApplication.QueuePlayerLoopUpdate();
        }

        private void OnInspectorDrawHeaderGUI(UnityEditor.Editor obj)
        {
            if (obj.target is GameObject or Component)
            {
                var prevColor = GUI.color;
                GUI.color = Color.yellow * new Color(1,1,1,0.1f);
                GUI.DrawTexture(new Rect(0,0, 100000,100000), Texture2D.whiteTexture);
                GUI.color = prevColor;
                EditorGUILayout.HelpBox($"A {nameof(Screenplay)} is currently being previewed, changes you introduce now may be rolled back once preview is done", MessageType.Warning);
            }
        }

        ~Previewer()
        {
            if (_rollbacksRegistered.Count > 0 || _players.Count > 0)
            {
                Debug.LogError("A previewer was not correctly disposed, your scene likely has changes introduced from a preview that cannot be rolled back");
            }
        }

        private void Update()
        {
            if (_players.Count > 0)
            {
                //SceneView.RepaintAll();
                EditorApplication.QueuePlayerLoopUpdate();
            }

            for (int i = 0; i < _players.Count; i++)
            {
                var (_, enumerator) = _players[i];
                if (enumerator.Current.Type is Signal.DelayType.SwapToAction or Signal.DelayType.SoftBreak)
                {
                }
                else if (enumerator.Current.Type != Signal.DelayType.NextFrame)
                    Debug.LogWarning($"{enumerator.Current.Type} is not supported as a preview type yet, it will be ignored");

                bool continues = false;
                try
                {
                    continues = enumerator.MoveNext();
                }
                finally
                {
                    if (continues == false)
                    {
                        try
                        {
                            enumerator.Dispose();
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                        // No reverse for loop as the order in which they have been scheduled may matter
                        _players.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        public void RunAsynchronously(object key, IEnumerable<Signal> runner)
        {
            StopAsynchronous(key);
            // ReSharper disable once GenericEnumeratorNotDisposed - Disposed in the outer loop
            var e = runner.GetEnumerator();
            e.MoveNext();
            _players.Add((key, e));
        }

        public bool StopAsynchronous(object key)
        {
            for (int i = 0; i < _players.Count; i++)
            {
                if (_players[i].key == key)
                {
                    _players[i].enumerator.Dispose();
                    _players.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void RegisterRollback(System.Action rollback)
        {
            _rollbacksRegistered.Push(rollback);
        }

        public void RegisterRollback(AnimationClip clip, GameObject go)
        {
            var animState = new AnimationRollback(go, clip);
            _rollbacksRegistered.Push(() => { animState.Rollback(); });
        }

        public void PlayCustomSignal(IEnumerable<Signal> signal)
        {
            var e = signal.GetEnumerator();
            e.MoveNext();
            _players.Add((null!, e));
        }
    }
}
