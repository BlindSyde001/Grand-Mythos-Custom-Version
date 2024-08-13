using System.Diagnostics.CodeAnalysis;

public interface IAction : IIdentifiable
{
    [MaybeNull] Condition TargetFilter { get; }
    [MaybeNull] Condition Precondition { get; }
    void Perform(BattleCharacterController[] targets, EvaluationContext context);
    string Name { get; }
    string Description { get; }
    float EnmityGenerationTarget { get; }
    float EnmityGenerationNonTarget { get; }
}