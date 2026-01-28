namespace Conditions
{
    public interface IConditionEvalTracker
    {
        void PostBeforeConditionEval(Condition condition, TargetCollection targets, EvaluationContext context);
        void PostAfterConditionEval(Condition condition, TargetCollection targetsBefore, TargetCollection targetsAfter, EvaluationContext context);
        void PostDead(CharacterTemplate source);
        void PostNotEnoughMana(CharacterTemplate source);
        void PostActionPrecondition(CharacterTemplate source, IAction action, TargetCollection allTargets);
        void PostActionTargetFilter(CharacterTemplate source, IAction action, TargetCollection previousTargets);
        void PostTargetFilter(CharacterTemplate source, Condition targetFilter);
        void PostAdditionalCondition(CharacterTemplate source, Condition condition, TargetCollection previousTargets);
        void PostSuccess(CharacterTemplate source, TargetCollection previousTargets);
    }
}