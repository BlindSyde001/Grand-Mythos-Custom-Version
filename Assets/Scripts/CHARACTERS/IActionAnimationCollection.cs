using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class IActionAnimationCollection : ISerializationCallbackReceiver
{
    [NonSerialized]
    readonly Dictionary<IAction, IActionAnimation> _collection = new();

    [SerializeField]
    [ListDrawerSettings(ShowFoldout = false)]
    [CanBeNull, InlineProperty]
    BackingKeyValue[] _backingArray;

    public void OnBeforeSerialize(){}

    public void OnAfterDeserialize()
    {
        if (_backingArray == null)
            return;

        _collection.Clear();
        foreach (var value in _backingArray)
        {
            if (value.Action == null)
                continue;

            if (_collection.TryAdd((IAction)value.Action, value.Animation) == false)
                Debug.LogWarning($"Could not bind '{value.Animation}' to action '{(IAction)value.Action}' - that action is already bound to '{_collection[(IAction)value.Action]}'");
        }
    }

    public bool TryGet(IAction action, out IActionAnimation animation) => _collection.TryGetValue(action, out animation);

    public bool Validate(CharacterTemplate template, ref string message)
    {
        if (_backingArray == null)
            return true;

        var actions = new Dictionary<IAction, string>();
        foreach (var tactic in template.Tactics)
            foreach (var action in tactic.Actions)
                actions.TryAdd(action, nameof(template.Tactics));

        if (template.LevelUnlocks != null)
            foreach (var unlock in template.LevelUnlocks.Skills)
                actions.TryAdd(unlock.Skill, nameof(template.LevelUnlocks));

        if (template.Inventory is not PlayerInventory)
        {
            foreach ((BaseItem item, uint count) in template.Inventory.Items())
                if (item is Consumable consumable)
                    actions.TryAdd(consumable, nameof(template.Inventory));
        }

        foreach (var keyValue in _backingArray)
        {
            if (keyValue.Action is not IAction action || keyValue.Animation is null)
                continue;

            if (keyValue.Animation.Validate(action, template, ref message) == false)
                return false;

            actions.Remove(action);
        }

        if (actions.Count > 0)
        {
            message = $"No animations setup for action {string.Join(", ", actions.Select(x => $"{x.Key.Name} from {x.Value}"))}";
            return false;
        }

        return true;
    }

    [Serializable]
    public struct BackingKeyValue
    {
        [ConstrainedType(typeof(IAction)), ValidateInput(nameof(IsIAction), "Must be an IAction, skill or consumable")]
        [HideLabel]
        public ScriptableObject Action;

        [SerializeReference, InlineProperty]
        [Required, HideLabel]
        public IActionAnimation Animation;

        bool IsIAction(ScriptableObject obj, ref string error)
        {
            if (obj is null)
            {
                error = "Must not be null";
                return false;
            }

            return obj is IAction;
        }
    }
}