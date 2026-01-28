using System;
using System.Linq;
using Sirenix.OdinInspector;
using UI.SkillTree;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UnlockNode : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeReference, BoxGroup("UNLOCKS"), HideLabel]
    public required IUnlock[] Unlocks;

    [FormerlySerializedAs("Requirements"), ListDrawerSettings(ShowFoldout = false)]
    public UnlockNode[] LinkedTo = Array.Empty<UnlockNode>();

    [BoxGroup("VISUALS")] public required Button Button;
    [BoxGroup("VISUALS")] public UnityEvent? OnReachable, OnUnreachable;
    [BoxGroup("VISUALS")] public Subnode[] Subnodes = new Subnode[6];
    UnlockNode[] _previousLinks = Array.Empty<UnlockNode>();

    protected void OnTransformParentChanged()
    {
        GetComponentInParent<SkillTree>()?.EnsureRegistered(this);
    }

    void OnValidate()
    {
        ForceValidateLinks();
        for (int i = 0; i < Subnodes.Length; i++)
            Subnodes[i].OnDisabled?.Invoke();
        for (int i = 0; i < Unlocks.Length; i++)
            Subnodes[Unlocks.Length - 1 - i].OnEnabled?.Invoke();
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        // We need to store the state of the links before they are changed to
        // detect editor changes
        if (_previousLinks.SequenceEqual(LinkedTo) == false)
            _previousLinks = LinkedTo.AsSpan().ToArray(); // Copy
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize(){ }

    [Button]
    public void ForceValidateLinks()
    {
        var newItems = LinkedTo.ToHashSet();
        var previousItems = _previousLinks.ToHashSet();
        foreach (var previousItem in previousItems)
        {
            if (newItems.Contains(previousItem) == false)
                previousItem.LinkedTo = previousItem.LinkedTo.Where(x => ReferenceEquals(x, this) == false).ToArray();
        }
        foreach (var newItem in newItems)
        {
            if (previousItems.Contains(newItem) == false && newItem.LinkedTo.Contains(this) == false)
                newItem.LinkedTo = newItem.LinkedTo.Append(this).ToArray();
        }

        _previousLinks = LinkedTo.AsSpan().ToArray(); // Copy
    }
}