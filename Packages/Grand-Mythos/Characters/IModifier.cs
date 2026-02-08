using Characters;

public interface IModifier
{
    ModifierDisplay? DisplayPrefab { get; }
    void ModifyStats(ref Stats stats);
    /// <summary>
    /// This modifier is currently attached to the caster and modifies damage the caster deals to the target.
    /// </summary>
    void ModifyOutgoingDelta(EvaluationContext context, CharacterTemplate target, ref ComputableDamageScaling scaling);
    /// <summary>
    /// This modifier is attached to the target of an action and modifies damage the <paramref name="target"/> receives from the caster.
    /// </summary>
    void ModifyIncomingDelta(EvaluationContext context, CharacterTemplate target, ref ComputableDamageScaling scaling);

    bool Temporary { get; }
    bool DisplayOnRightSide { get; }
    bool IsStillValid(AppliedModifier data, EvaluationContext context);
}