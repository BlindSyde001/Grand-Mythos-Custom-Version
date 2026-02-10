using UnityEngine;

public interface IAction : IIdentifiable
{
    string Name { get; }
    string Description { get; }
    int ManaCost { get; }
    float FlowCost { get; }
    Delay DelayToNextTurn { get; }

    AnimationClip? CameraAnimation { get; }
    Condition? TargetFilter { get; }
    Condition? Precondition { get; }
    void Perform(CharacterTemplate[] targets, EvaluationContext context);

    public enum Delay
    {
        Short,
        Base,
        Long
    }
}