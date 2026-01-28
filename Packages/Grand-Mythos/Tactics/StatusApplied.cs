using System;
using System.Diagnostics.CodeAnalysis;
using StatusHandler;

namespace Conditions
{
    [Serializable]
    public class StatusApplied : SimplifiedCondition
    {
        public bool Has = true;
        public required StatusModifier Status;

        protected override bool Filter(BattleCharacterController target, EvaluationContext context) => target.Profile.HasStatus(Status) == Has;

        public override bool IsValid([MaybeNullWhen(true)] out string error)
        {
            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context){}

        public override string UIDisplayText => Has ? $"has {Status}" : $"does not have {Status}";
    }
}