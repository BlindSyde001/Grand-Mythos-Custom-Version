using System.Diagnostics.CodeAnalysis;
using Characters;

public interface IModifier
{
    [MaybeNull] ModifierDisplay DisplayPrefab { get; }
    void ModifyStats(ref Stats stats);
    /// <summary>
    /// This modifier is currently attached to the caster and modifies damage the caster deals to the target.
    /// You can retrieve the reference to the caster by fetching <see cref="EvaluationContext.Controller"/>.
    /// </summary>
    void ModifyOutgoingDelta(EvaluationContext context, BattleCharacterController target, ref ComputableDamageScaling scaling);
    /// <summary>
    /// This modifier is attached to the target of an action and modifies damage the <see cref="target"/> receives from the caster.
    /// You can retrieve the reference to the caster by fetching <see cref="EvaluationContext.Controller"/>.
    /// </summary>
    void ModifyIncomingDelta(EvaluationContext context, BattleCharacterController target, ref ComputableDamageScaling scaling);

    bool Temporary { get; }
    bool IsStillValid(AppliedModifier data, EvaluationContext context);
}