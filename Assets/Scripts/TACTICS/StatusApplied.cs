using System;
using StatusHandler;

namespace Conditions
{
    [Serializable]
    public class StatusApplied : SimplifiedCondition
    {
        public StatusModifier Status;

        protected override bool Filter(BattleCharacterController target, EvaluationContext context) => target.Profile.HasStatus(Status);

        public override bool IsValid(out string error)
        {
            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context){}

        public override string UIDisplayText => $"has {Status}";
    }
}