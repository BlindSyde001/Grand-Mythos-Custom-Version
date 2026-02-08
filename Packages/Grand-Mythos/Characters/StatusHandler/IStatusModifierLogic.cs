namespace StatusHandler
{
    public interface IStatusModifierLogic
    {
        void Modify(EvaluationContext context, CharacterTemplate target, ref ComputableDamageScaling scaling);
    }
}