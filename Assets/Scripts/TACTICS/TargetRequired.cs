using System;
using Sirenix.OdinInspector;

namespace Conditions
{
    [Serializable]
    public class TargetRequired : Condition
    {
        [HorizontalGroup, HideLabel]
        public ComparisonType Comparison = ComparisonType.GreaterThan;
        [HorizontalGroup, HideLabel, SuffixLabel("target(s)")]
        public int Amount = 1;
        protected override void FilterInner(ref TargetCollection targets, EvaluationContext context)
        {
            var amount = targets.CountSlow();
            bool result = Comparison switch
            {
                ComparisonType.LesserThan => amount < Amount,
                ComparisonType.LessOrEqualTo => amount <= Amount,
                ComparisonType.EqualTo => amount == Amount,
                ComparisonType.GreaterOrEqualTo => amount >= Amount,
                ComparisonType.GreaterThan => amount > Amount,
                ComparisonType.NotEqualTo => amount != Amount,
                _ => throw new ArgumentOutOfRangeException()
            };
            if (result == false)
                targets.Empty();
        }

        public override bool IsValid(out string error)
        {
            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context){}

        public override string UIDisplayText => $"number of targets {Comparison} {Amount}";
    }
}