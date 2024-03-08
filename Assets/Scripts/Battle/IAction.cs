using System.Collections;

public interface IAction
{
    uint ATBCost { get; }
    Condition TargetFilter { get; }
    Condition Precondition { get; }
    IEnumerable Perform(BattleCharacterController[] targets, EvaluationContext context);
    string Name { get; }
}