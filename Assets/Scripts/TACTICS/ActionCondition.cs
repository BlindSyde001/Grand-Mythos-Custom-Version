using System;
using System.Diagnostics.CodeAnalysis;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class ActionCondition : IdentifiableScriptableObject
{
    [BoxGroup(nameof(TargetFilter), LabelText = @"@""TargetFilter:   "" + this.TargetFilter?.UIDisplayText")]
    [InfoBox("If at least one unit matches this condition, the AI will execute the action on them")]
    [ValidateInput(nameof(ValidateCondition), "")]
    [HideLabel]
    [SerializeReference]
    [MaybeNull]
    public Condition TargetFilter;

    [BoxGroup(nameof(AdditionalCondition), LabelText = @"@""AdditionalCondition:   "" + this.AdditionalCondition?.UIDisplayText")]
    [InfoBox("Condition that must be true in general")]
    [HideLabel]
    [SerializeReference]
    [MaybeNull]
    public Condition AdditionalCondition;

    bool ValidateCondition(Condition _, ref string errorMessage)
    {
        return IsValid(out errorMessage);
    }

    public bool IsValid(out string error)
    {
        if (TargetFilter != null && TargetFilter.IsValid(out error) == false)
            return false;

        if (AdditionalCondition != null && AdditionalCondition.IsValid(out error) == false)
            return false;

        error = null;
        return true;
    }

    public bool CanExecute(ReadOnlySpan<IAction> actions, TargetCollection allTargets, EvaluationContext context, out TargetCollection selection, bool accountForCost)
    {
        selection = default;

        context.ExecutionFlags.Clear();

        if (accountForCost && actions.CostTotal() > context.Controller.Profile.ActionsCharged)
            return false;

        foreach (var action in actions)
        {
            if (action.Precondition != null)
            {
                var allTargetsCopy = allTargets;
                try
                {
                    action.Precondition.Filter(ref allTargetsCopy, context.Controller.Context);
                }
                catch(Exception e) when (action is UnityEngine.Object o)
                {
                    Debug.LogException(e, o);
                }
                if (allTargetsCopy.IsEmpty)
                    return false;
            }
        }

        var selectedTargets = allTargets;
        if (TargetFilter != null)
        {
            try
            {
                TargetFilter.Filter(ref selectedTargets, context);
            }
            catch(Exception e)
            {
                Debug.LogException(e, this);
            }
            if (selectedTargets.IsEmpty)
                return false;
        }

        foreach (var action in actions)
        {
            if (action.TargetFilter != null)
            {
                try
                {
                    action.TargetFilter.Filter(ref selectedTargets, context);
                }
                catch(Exception e) when (action is UnityEngine.Object o)
                {
                    Debug.LogException(e, o);
                }
                if (selectedTargets.IsEmpty)
                    return false;
            }
        }

        if (AdditionalCondition != null)
        {
            var allTargetsCopy = allTargets;
            try
            {
                AdditionalCondition.Filter(ref allTargetsCopy, context);
            }
            catch(Exception e)
            {
                Debug.LogException(e, this);
            }
            if (allTargetsCopy.IsEmpty)
                return false;
        }

        selection = selectedTargets;
        return true;
    }
}

[Serializable]
public class EvaluationContext
{
    [SerializeField]
    BattleCharacterController _controller;

    public BattleCharacterController Controller => _controller;
    public CharacterTemplate Profile => Controller.Profile;

    /// <summary>
    /// Reset only between evaluating full tactics,
    /// values are kept when going through the different evaluations;
    /// values set in <see cref="IAction.Precondition"/> can be checked in <see cref="ActionCondition.TargetFilter"/>, etc.
    /// </summary>
    public SerializableDictionary<object, object> ExecutionFlags = new();
#warning reset after combat
    /// <summary> Reset before every battle </summary>
    public SerializableDictionary<object, object> BattleFlags = new();

    /// <summary>
    /// Incremented every time <see cref="Controller"/> has finished playing all its scheduled tactics
    /// </summary>
    public uint Round;

#warning bind combat seed
    public uint CombatSeed;

    public EvaluationContext(BattleCharacterController controller)
    {
        _controller = controller;
    }
}