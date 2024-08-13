using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class Tactics
{
    [TableColumnWidth(48, resizable:false)]
    public bool IsOn = true;
    [TableColumnWidth(200, resizable:false)]
    public ActionCondition Condition;

    public IAction Action 
    {
        get => (IAction)ActionsAsset;
        set => ActionsAsset = (ScriptableObject)value;
    }

    [ShowInInspector]
    [SerializeField]
    [ValidateInput(nameof(IsIAction), "Must be an IAction, skill or consumable")]
    [ConstrainedType(typeof(IAction))]
    ScriptableObject ActionsAsset;

    bool IsIAction(ScriptableObject obj, ref string error)
    {
        if (obj is not IAction)
        {
            error = $"{obj} is not an IAction, skill or consumable";
            return false;
        }

        return true;
    }
}