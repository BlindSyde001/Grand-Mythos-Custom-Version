using System.Diagnostics.CodeAnalysis;

namespace Conditions
{
    public class IsAlive : SimplifiedCondition
    {
        public override string UIDisplayText => "is alive";

        public override bool IsValid([MaybeNullWhen(true)] out string error)
        {
            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context){ }

        protected override bool Filter(CharacterTemplate target, EvaluationContext context)
        {
            return target.CurrentHP > 0;
        }
    }
}