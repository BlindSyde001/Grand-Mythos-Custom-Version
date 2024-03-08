using System.Collections;
using System.Diagnostics.CodeAnalysis;

public interface IAction
{
    uint ATBCost { get; }
    [MaybeNull] Condition TargetFilter { get; }
    [MaybeNull] Condition Precondition { get; }
    IEnumerable Perform(BattleCharacterController[] targets, EvaluationContext context);
    string Name { get; }
}