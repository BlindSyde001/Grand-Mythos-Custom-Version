namespace StatusHandler
{
    public interface IStatusModifierLogic
    {
        void Modify(EvaluationContext context, BattleCharacterController target, ref ComputableDamageScaling scaling);
    }
}