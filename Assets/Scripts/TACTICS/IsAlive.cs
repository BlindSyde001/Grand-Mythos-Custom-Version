namespace Conditions
{
    public class IsAlive : SimplifiedCondition
    {
        public override string UIDisplayText => "is alive";

        public override bool IsValid(out string error)
        {
            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context){ }

        protected override bool Filter(BattleCharacterController target, EvaluationContext context)
        {
            return target.Profile.CurrentHP > 0;
        }
    }
}