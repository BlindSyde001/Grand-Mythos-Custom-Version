using System.Collections;
using JetBrains.Annotations;

public interface IAction
{
    uint ActionCost { get; }
    [CanBeNull] Condition TargetFilter { get; }
    [CanBeNull] Condition Precondition { get; }
    void Perform(BattleCharacterController[] targets, EvaluationContext context);
    string Name { get; }
    string Description { get; }
}