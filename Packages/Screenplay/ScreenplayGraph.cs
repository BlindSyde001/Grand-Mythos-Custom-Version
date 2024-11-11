using System;
using System.Collections.Generic;
using UnityEngine;
using YNode;
using Screenplay.Nodes;
using Screenplay.Nodes.Triggers;
using Event = Screenplay.Nodes.Event;

namespace Screenplay
{
    [CreateAssetMenu(menuName = "Screenplay/Screenplay")]
    public class ScreenplayGraph : NodeGraph, ISerializationCallbackReceiver
    {
        public Component.DialogUIComponent? DialogUIPrefab;
        public bool DebugRetainProgressInEditor;
        private HashSet<IPrerequisite> _visited = new();
        private HashSet<Event> _visitedEvents = new();

        // Only serialized for editor reloading purposes
        [SerializeField, HideInInspector] private IAction? _action;
        [SerializeField, HideInInspector, SerializeReference] private List<IPrerequisite> __visitedSerializationProxy = new ();
        [SerializeField, HideInInspector, SerializeReference] private List<Event> __visitedEventsSerializationProxy = new ();

        /// <summary>
        /// You must dispose of this enumerator when reloading a running game.
        /// </summary>
        public IEnumerator<object?> StartExecution()
        {
            using var context = new DefaultContext(this);
            var events = new List<Event>();
            var triggers = new Dictionary<Event, ITrigger>();
            try
            {
                foreach (var value in Nodes)
                {
                    if (value is Event e && e.Action is not null && (e.Repeatable || _visitedEvents.Contains(e) == false))
                        events.Add(e);
                }

                do
                {
                    if (_action is null) // Check non-triggerable
                    {
                        foreach (var e in events)
                        {
                            if (e.TriggerSource is not null)
                                continue;
                            if (e.Prerequisite?.TestPrerequisite(_visited) == false)
                                continue;

                            _visitedEvents.Add(e);
                            if (e.Repeatable == false)
                                events.Remove(e);
                            _action = e.Action;
                            break;
                        }
                    }

                    if (_action is null) // Check triggerable
                    {
                        foreach (var e in events)
                        {
                            if (e.TriggerSource is null)
                                continue;

                            if (e.Prerequisite?.TestPrerequisite(_visited) == false)
                            {
                                if (triggers.TryGetValue(e, out var outdatedTrigger))
                                    outdatedTrigger.Dispose();
                                continue;
                            }

                            if (triggers.ContainsKey(e))
                                continue;

                            System.Action callback = () =>
                            {
                                if (_action is not null)
                                    return; // Another trigger already queued one

                                _visitedEvents.Add(e);
                                if (e.Repeatable == false)
                                    events.Remove(e);
                                _action = e.Action;
                            };

                            if (e.TriggerSource.TryCreateTrigger(callback, out var trigger) == false)
                                continue;

                            triggers.Add(e, trigger);
                            break;
                        }
                    }

                    if (_action is null)
                    {
                        context.AsynchronousTick();
                        yield return null;
                        continue;
                    }

                    foreach (var (_, trigger) in triggers)
                        trigger.Dispose();
                    triggers.Clear();

                    _visited.Add(_action);
                    var currentAction = _action;
                    _action = null;
                    foreach (var signal in currentAction.Execute(context))
                    {
                        if (signal.Type == Signal.DelayType.SoftBreak)
                        {
                            break;
                        }
                        else if (signal.Type == Signal.DelayType.NextFrame)
                        {
                            context.AsynchronousTick();
                            yield return null;
                        }
                        else if (signal.Type == Signal.DelayType.SwapToAction)
                        {
                            _action = signal.Action;
                            break;
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException(nameof(signal), signal.Type, "Unimplemented signal type");
                        }
                    }
                } while (true);
            }
            finally
            {
                foreach (var (_, trigger) in triggers)
                    trigger.Dispose();
            }
        }

        public bool Visited(IPrerequisite node) => _visited.Contains(node);

        public IEnumerable<LocalizableText> GetLocalizableText()
        {
            foreach (var node in Nodes)
            {
                if (node is ILocalizableNode localizable)
                {
                    foreach (var localizableText in localizable.GetTextInstances())
                    {
                        yield return localizableText;
                    }
                }
            }
        }

        [ThreadStatic]
        private static HashSet<IAction>? _isNodeReachableVisitation;
        public bool IsNodeReachable(IAction thisAction, List<INodeValue>? path = null)
        {
            _isNodeReachableVisitation ??= new();
            _isNodeReachableVisitation.Clear();
            foreach (var node in Nodes)
            {
                if (node is Event e && e.Action is not null)
                {
                    path?.Add(e);
                    if (FindLeafAInBranchB(thisAction, e.Action, path))
                        return true;
                    path?.RemoveAt(path.Count-1);
                }

                static bool FindLeafAInBranchB(IAction target, IAction branch, List<INodeValue>? path)
                {
                    if (_isNodeReachableVisitation!.Add(branch) == false)
                        return false;

                    if (target == branch)
                    {
                        path?.Add(target);
                        return true;
                    }

                    path?.Add(branch);
                    foreach (IAction otherActions in branch.Followup())
                    {
                        if (FindLeafAInBranchB(target, otherActions, path))
                            return true;
                    }
                    path?.RemoveAt(path.Count-1);

                    return false;
                }
            }

            return false;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            __visitedEventsSerializationProxy.Clear();
            __visitedSerializationProxy.Clear();
            __visitedSerializationProxy.AddRange(_visited);
            __visitedEventsSerializationProxy.AddRange(_visitedEvents);

            var guids = new Dictionary<Guid, LocalizableText>();
            foreach (var localizableText in GetLocalizableText())
            {
                while (guids.TryGetValue(localizableText.Guid, out var existingInstance) && existingInstance != localizableText)
                {
                    localizableText.ForceRegenerateGuid();
                    Debug.LogWarning($"Duplicate Guid detected between '{localizableText.Content}' and '{existingInstance.Content}', regenerating Guid. This is standard behavior when copying nodes");
                }
                guids[localizableText.Guid] = localizableText;
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // This is to retain progress when reloading in play mode
            foreach (var prerequisite in __visitedSerializationProxy)
                _visited.Add(prerequisite);
            foreach (var e in __visitedEventsSerializationProxy)
                _visitedEvents.Add(e);
        }

#if UNITY_EDITOR
        static ScreenplayGraph()
        {
            UnityEditor.EditorApplication.playModeStateChanged += pmstc =>
            {
                if (pmstc is not (UnityEditor.PlayModeStateChange.ExitingEditMode or UnityEditor.PlayModeStateChange.ExitingPlayMode))
                {
                    return;
                }

                foreach (var screenplay in Resources.FindObjectsOfTypeAll<ScreenplayGraph>())
                {
                    if (screenplay.DebugRetainProgressInEditor)
                        continue;
                    screenplay._action = null;
                    screenplay.__visitedSerializationProxy.Clear();
                    screenplay.__visitedEventsSerializationProxy.Clear();
                    screenplay._visited.Clear();
                    screenplay._visitedEvents.Clear();
                }
            };
        }
#endif

        private record DefaultContext(ScreenplayGraph Source) : IContext, IDisposable
        {
            private List<(object, IEnumerator<Signal>)> _asynchronousRunner = new();
            private Component.DialogUIComponent? _dialogUI;

            public ScreenplayGraph Source { get; } = Source;

            public HashSet<IPrerequisite> Visited => Source._visited;

            public void RunAsynchronously(object key, IEnumerable<Signal> runner)
            {
                StopAsynchronous(key);
                // ReSharper disable once GenericEnumeratorNotDisposed - Disposed in the outer loop
                var e = runner.GetEnumerator();
                e.MoveNext();
                _asynchronousRunner.Add((key, e));
            }

            public bool StopAsynchronous(object key)
            {
                for (int i = 0; i < _asynchronousRunner.Count; i++)
                {
                    if (_asynchronousRunner[i].Item1 == key)
                    {
                        _asynchronousRunner[i].Item2.Dispose();
                        _asynchronousRunner.RemoveAt(i);
                        return true;
                    }
                }

                return false;
            }

            public void AsynchronousTick()
            {
                for (int i = 0; i < _asynchronousRunner.Count; i++)
                {
                    var (key, enumerator) = _asynchronousRunner[i];
                    if (enumerator.Current.Type is Signal.DelayType.SwapToAction or Signal.DelayType.SoftBreak)
                        Debug.LogError($"{enumerator.Current.Type} is not allowed for Asynchronous runner");
                    else if (enumerator.Current.Type != Signal.DelayType.NextFrame)
                        Debug.LogError($"Asynchronous does not handle {enumerator.Current.Type} yet");

                    bool continues = false;
                    try
                    {
                        continues = enumerator.MoveNext();
                    }
                    finally
                    {
                        if (continues == false) // Whether because MoveNext returned false or threw
                        {
                            try
                            {
                                enumerator.Dispose();
                            }
                            finally
                            {
                                _asynchronousRunner.RemoveAt(i);
                            }
                        }
                    }
                }
            }

            public Component.UIBase? GetDialogUI()
            {
                return _dialogUI != null ? _dialogUI : _dialogUI = Instantiate(Source.DialogUIPrefab);
            }

            public void Dispose()
            {
                foreach (var (_, runner) in _asynchronousRunner)
                    runner.Dispose();
                if (_dialogUI != null)
                    Destroy(_dialogUI.gameObject);
            }
        }
    }
}
