using System.Diagnostics.CodeAnalysis;
using Battle;

public interface IAction : IIdentifiable
{
    float ChargeDuration { get; }
    Channeling Channeling { get; }
    [MaybeNull] Condition TargetFilter { get; }
    [MaybeNull] Condition Precondition { get; }
    void Perform(BattleCharacterController[] targets, EvaluationContext context);
    string Name { get; }
    string Description { get; }
}