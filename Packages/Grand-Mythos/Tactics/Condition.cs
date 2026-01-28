using System.Diagnostics.CodeAnalysis;
using Sirenix.OdinInspector;
using Conditions;


[InlineProperty]
public abstract class Condition
{
    /// <summary> Describes this condition in a human-readable format </summary>
    public abstract string UIDisplayText { get; }

    public void Filter(ref TargetCollection targets, EvaluationContext context)
    {
        if (context.Tracker == null)
        {
            FilterInner(ref targets, context);
        }
        else
        {
            var targetsPreFiltering = targets;
            context.Tracker.PostBeforeConditionEval(this, targetsPreFiltering, context);
            FilterInner(ref targets, context);
            context.Tracker.PostAfterConditionEval(this, targetsPreFiltering, targets, context);
        }
    }

    /// <summary>
    /// Given the targets provided, remove all targets that do not match its condition
    /// </summary>
    protected abstract void FilterInner(ref TargetCollection targets, EvaluationContext context);

    /// <summary>
    /// Checks whether this condition is configured properly in the editor
    /// </summary>
    public abstract bool IsValid([MaybeNullWhen(true)] out string error);

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