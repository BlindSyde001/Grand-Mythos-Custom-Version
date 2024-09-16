using System.Diagnostics.CodeAnalysis;
using Battle;
using UnityEngine;

public interface IAction : IIdentifiable
{
    string Name { get; }
    string Description { get; }
    int ManaCost { get; }
    float FlowCost { get; }
    float ChargeDuration { get; }
    Channeling Channeling { get; }
    [MaybeNull] AnimationClip CameraAnimation { get; }
    [MaybeNull] Condition TargetFilter { get; }
    [MaybeNull] Condition Precondition { get; }
    void Perform(BattleCharacterController[] targets, EvaluationContext context);
}