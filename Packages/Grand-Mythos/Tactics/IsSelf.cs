using System;
using System.Diagnostics.CodeAnalysis;

namespace Conditions
{
    [Serializable]
    public class IsSelf : SimplifiedCondition
    {
        protected override bool Filter(CharacterTemplate target, EvaluationContext context) => context.Profile == target;

        public override bool IsValid([MaybeNullWhen(true)] out string error)
        {
            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context){}

        public override string UIDisplayText => "is myself";
    }
}