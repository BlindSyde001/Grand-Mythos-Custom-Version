using System.Collections;

public interface IAction
{
    uint ATBCost { get; }
    Condition TargetFilter { get; }
    Condition Precondition { get; }
    IEnumerable Perform(TargetCollection targets, EvaluationContext context);
    string Name { get; }
}