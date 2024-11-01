using System;

namespace Conditions
{
    [Serializable]
    public class OncePerCombat : Condition
    {
        protected override void FilterInner(ref TargetCollection targets, EvaluationContext context)
        {
            if (context.BattleFlags.ContainsKey(this))
            {
                targets.Empty();
                return;
            }
        }

        public override bool IsValid(out string error)
        {
            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context)
        {
            context.BattleFlags.Add(this, null);
        }

        public override string UIDisplayText => "once per combat";
    }
}