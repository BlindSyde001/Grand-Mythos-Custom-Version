using System.Diagnostics.CodeAnalysis;

public interface IModifier
{
    [MaybeNull] ModifierDisplay DisplayPrefab { get; }
    void ModifyStats(ref Stats stats);
    void ModifyOutgoingDelta(EvaluationContext context, BattleCharacterController target, ref ComputableDamageScaling scaling);
    void ModifyIncomingDelta(EvaluationContext context, BattleCharacterController target, ref ComputableDamageScaling scaling);
}