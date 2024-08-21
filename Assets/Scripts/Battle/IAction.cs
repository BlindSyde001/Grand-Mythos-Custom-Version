using System.Diagnostics.CodeAnalysis;

public interface IAction : IIdentifiable
{
    float ChargeDuration { get; }
    [MaybeNull] Condition TargetFilter { get; }
    [MaybeNull] Condition Precondition { get; }
    void Perform(BattleCharacterController[] targets, EvaluationContext context);
    string Name { get; }
    string Description { get; }
}