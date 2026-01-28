using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class Tactics
{
    [TableColumnWidth(48, resizable:false)]
    public bool IsOn = true;
    [TableColumnWidth(200, resizable:false)]
    public required ActionCondition Condition;

    public IAction Action 
    {
        get => (IAction)ActionsAsset;
        set => ActionsAsset = (ScriptableObject)value;
    }

    [ShowInInspector]
    [SerializeField, Required]
    [ValidateInput(nameof(IsIAction), "Must be an IAction, skill or consumable")]
    [ConstrainedType(typeof(IAction))]
    ScriptableObject ActionsAsset = null!;

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