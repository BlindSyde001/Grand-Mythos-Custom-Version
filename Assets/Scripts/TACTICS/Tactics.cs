﻿using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class Tactics
{
    [SerializeReference] public ActionCondition Condition;
    [HideLabel] public IActionCollection Actions = new();
}

[Serializable, InlineProperty]
public class IActionCollection : ISerializationCallbackReceiver, IEnumerable<IAction>
{
    [NonSerialized] public IAction[] BackingArray = Array.Empty<IAction>();

    [ShowInInspector]
    [SerializeField]
    [ValidateInput(nameof(IsIAction), "Must be an IAction, skill or consumable")]
    [ConstrainedType(typeof(IAction))]
    [LabelText(@"@""Actions ("" + this.CostTotal() + "" ATB)""")]
    [ListDrawerSettings(ShowFoldout = false, CustomAddFunction = "CustomAddFunction")]
    ScriptableObject[] ActionsAssets;

    public IAction this[int index]
    {
        get => BackingArray[index];
        set => BackingArray[index] = value;
    }

    public int Length => BackingArray.Length;

    public void OnBeforeSerialize(){}

    public void OnAfterDeserialize()
    {
        BackingArray = new IAction[ActionsAssets.Length];
        for (int i = 0; i < ActionsAssets.Length; i++)
            BackingArray[i] = (IAction)ActionsAssets[i];
    }

    bool IsIAction(ScriptableObject[] obj, ref string error)
    {
        for (int i = 0; i < obj.Length; i++)
        {
            if (obj[i] is not IAction)
            {
                error = $"#{i} is not an IAction, skill or consumable";
                return false;
            }
        }

        return true;
    }

    ScriptableObject CustomAddFunction()
    {
        return ActionsAssets == null || ActionsAssets.Length == 0 ? null : ActionsAssets[^1];
    }

    public uint CostTotal()
    {
        uint cost = 0;
        if (BackingArray == null)
            return 0;

        foreach (var action in BackingArray)
            cost += action?.ATBCost ?? 0;

        return cost;
    }

    public Enumerator<IAction> GetEnumerator() => new(BackingArray);
    IEnumerator<IAction> IEnumerable<IAction>.GetEnumerator() => new Enumerator<IAction>(BackingArray);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator<T> : IEnumerator<T>
    {
        readonly T[] collection;
        readonly int length;
        int index;

        public T Current => collection[index];
        object IEnumerator.Current => Current;

        public Enumerator(T[] arr)
        {
            collection = arr;
            length = arr.Length;
            index = -1;
        }

        public bool MoveNext()
        {
            index++;
            return index < length;
        }

        public void Reset()
        {
            index = -1;
        }

        public void Dispose() {}
    }
}