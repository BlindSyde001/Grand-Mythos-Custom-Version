using System.Collections.Generic;
using Sirenix.OdinInspector;
using Conditions;


[InlineProperty]
public abstract class Condition
{
    public static bool Track;
    public static CharacterTemplate Source;
    public static Dictionary<Condition, BattleCharacterController[]> DebugData = new();
    public static uint CurrentRound;

    /// <summary> Describes this condition in a human-readable format </summary>
    public abstract string UIDisplayText { get; }

    public void Filter(ref TargetCollection targets, EvaluationContext context)
    {
        Tracking(targets, context);
        FilterInner(ref targets, context);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public void Tracking(in TargetCollection targets, EvaluationContext context)
    {
        if (Track == false)
            return;

        if (Source != context.Controller.Profile)
            return;

        if (context.Round != CurrentRound)
            DebugData.Clear();
        CurrentRound = context.Round;

        DebugData.Add(this, targets.ToArray());
    }

    /// <summary>
    /// Given the targets provided, remove all targets that do not match its condition
    /// </summary>
    protected abstract void FilterInner(ref TargetCollection targets, EvaluationContext context);

    /// <summary>
    /// Checks whether this condition is configured properly in the editor
    /// </summary>
    public abstract bool IsValid(out string error);

    /// <summary>
    /// Called after an action linked to this condition ran
    /// </summary>
    public abstract void NotifyUsedCondition(in TargetCollection target, EvaluationContext context);

    public static And operator&(Condition left, Condition right)
    {
        return new And
        {
            Left = left,
            Right = right,
        };
    }

    public static Or operator|(Condition left, Condition right)
    {
        return new Or
        {
            Left = left,
            Right = right,
        };
    }
}