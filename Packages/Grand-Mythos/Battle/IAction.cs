using System.Diagnostics.CodeAnalysis;
using QTE;
using UnityEngine;

public interface IAction : IIdentifiable
{
    string Name { get; }
    string Description { get; }
    int ManaCost { get; }
    Delay DelayToNextTurn { get; }

    [MaybeNull] AnimationClip CameraAnimation { get; }
    [MaybeNull] Condition TargetFilter { get; }
    [MaybeNull] Condition Precondition { get; }
    void Perform(BattleCharacterController[] targets, QTEResult result, EvaluationContext context);

    public enum Delay
    {
        Short,
        Base,
        Long
    }
}