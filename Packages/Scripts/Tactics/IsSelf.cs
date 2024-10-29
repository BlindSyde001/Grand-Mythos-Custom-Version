using System;

namespace Conditions
{
    [Serializable]
    public class IsSelf : SimplifiedCondition
    {
        protected override bool Filter(BattleCharacterController target, EvaluationContext context) => context.Controller == target;

        public override bool IsValid(out string error)
        {
            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context){}

        public override string UIDisplayText => "is myself";
    }
}