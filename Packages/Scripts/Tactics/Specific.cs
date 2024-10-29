using System.Collections.Generic;

namespace Conditions
{
    public class SpecificTargetsCondition : SimplifiedCondition
    {
        public HashSet<BattleCharacterController> Targets = new();
        public override string UIDisplayText => "Specific Targets";

        public override bool IsValid(out string error)
        {
            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context){ }

        protected override bool Filter(BattleCharacterController target, EvaluationContext context) => Targets.Contains(target);
    }
}