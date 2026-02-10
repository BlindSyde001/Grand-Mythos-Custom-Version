using System;
using System.Diagnostics.CodeAnalysis;
using Conditions;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[CreateAssetMenu]
public class ActionCondition : IdentifiableScriptableObject
{
    [BoxGroup(nameof(TargetFilter), LabelText = @"@""TargetFilter:   "" + this.TargetFilter?.UIDisplayText")]
    [Tooltip("If at least one unit matches this condition, the AI will execute the action on them")]
    [ValidateInput(nameof(ValidateCondition), "")]
    [HideLabel]
    [SerializeReference]
    public Condition? TargetFilter;

    [BoxGroup(nameof(AdditionalCondition), LabelText = @"@""AdditionalCondition:   "" + this.AdditionalCondition?.UIDisplayText")]
    [Tooltip("Condition that must be true in general")]
    [HideLabel]
    [SerializeReference]
    public Condition? AdditionalCondition;

    bool ValidateCondition(Condition _, ref string? errorMessage)
    {
        return IsValid(out errorMessage);
    }

    public bool IsValid([MaybeNullWhen(true)] out string error)
    {
        if (TargetFilter != null && TargetFilter.IsValid(out error) == false)
            return false;

        if (AdditionalCondition != null && AdditionalCondition.IsValid(out error) == false)
            return false;

        error = null;
        return true;
    }

    public bool CanExecute(IAction action, TargetCollection allTargets, EvaluationContext context, out TargetCollection selection)
    {
        selection = default;

        context.ExecutionFlags.Clear();

        if (context.Profile.CurrentHP == 0)
        {
            context.Tracker?.PostDead(context.Profile);
            return false;
        }

        if (action.ManaCost > context.Profile.CurrentMP)
        {
            context.Tracker?.PostNotEnoughMana(context.Profile);
            return false;
        }

        if (action.FlowCost > context.Profile.CurrentFlow)
        {
            context.Tracker?.PostNotEnoughFlow(context.Profile);
            return false;
        }

        if (action.Precondition != null)
        {
            var allTargetsCopy = allTargets;
            try
            {
                action.Precondition.Filter(ref allTargetsCopy, context);
            }
            catch(Exception e) when (action is UnityEngine.Object o)
            {
                Debug.LogException(e, o);
            }

            if (allTargetsCopy.IsEmpty)
            {
                context.Tracker?.PostActionPrecondition(context.Profile, action, allTargets);
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
            {
                context.Tracker?.PostTargetFilter(context.Profile, TargetFilter);
                return false;
            }
        }

        if (action.TargetFilter != null)
        {
            var previousTargets = selectedTargets;
            try
            {
                action.TargetFilter.Filter(ref selectedTargets, context);
            }
            catch(Exception e) when (action is UnityEngine.Object o)
            {
                Debug.LogException(e, o);
            }

            if (selectedTargets.IsEmpty)
            {
                context.Tracker?.PostActionTargetFilter(context.Profile, action, previousTargets);
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
            {
                context.Tracker?.PostAdditionalCondition(context.Profile, AdditionalCondition, allTargetsCopy);
                return false;
            }
        }

        selection = selectedTargets;
        context.Tracker?.PostSuccess(context.Profile, selection);
        return true;
    }
}

[Serializable]
public class EvaluationContext
{
    public CharacterTemplate Profile;

    /// <summary>
    /// Reset only between evaluating full tactics,
    /// values are kept when going through the different evaluations;
    /// values set in <see cref="IAction.Precondition"/> can be checked in <see cref="ActionCondition.TargetFilter"/>, etc.
    /// </summary>
    public SerializableDictionary<object, object> ExecutionFlags = new();
    /// <summary> Reset before every battle </summary>
    public SerializableDictionary<object, object?> BattleFlags = new();

    /// <summary>
    /// Incremented every time <see cref="Profile"/> has finished playing all its scheduled tactics
    /// </summary>
    public uint Round;

    public uint CombatSeed;
    public Random Random;
    public double CombatTimestamp;
    [NonSerialized] public IConditionEvalTracker? Tracker;

    public EvaluationContext(CharacterTemplate template)
    {
        Profile = template;
        Random = new Random(1);
    }
}