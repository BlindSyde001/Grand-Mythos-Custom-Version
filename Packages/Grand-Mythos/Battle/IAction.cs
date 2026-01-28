using UnityEngine;

public interface IAction : IIdentifiable
{
    string Name { get; }
    string Description { get; }
    int ManaCost { get; }
    Delay DelayToNextTurn { get; }

    AnimationClip? CameraAnimation { get; }
    Condition? TargetFilter { get; }
    Condition? Precondition { get; }
    void Perform(BattleCharacterController[] targets, EvaluationContext context);

    public enum Delay
    {
        Short,
        Base,
        Long
    }
}