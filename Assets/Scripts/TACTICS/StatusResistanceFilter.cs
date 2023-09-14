using System;
using Sirenix.OdinInspector;

namespace Conditions
{
    [Serializable]
    public class StatusResistanceFilter : SimplifiedCondition
    {
        [HorizontalGroup, SuffixLabel("is")] public Status ResistanceTo;
        [HorizontalGroup, HideLabel] public ComparisonType Comparison;
        [HorizontalGroup, HideLabel] public int Value;

        protected override bool Filter(CharacterTemplate target, EvaluationContext context)
        {
            int stat = target.GetStatusResistance(ResistanceTo);
            return Comparison switch
            {
                ComparisonType.LesserThan => stat < Value,
                ComparisonType.LessOrEqualTo => stat <= Value,
                ComparisonType.EqualTo => stat == Value,
                ComparisonType.GreaterOrEqualTo => stat >= Value,
                ComparisonType.GreaterThan => stat > Value,
                ComparisonType.NotEqualTo => stat != Value,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override bool IsValid(out string error)
        {
            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context){}

        public override string UIDisplayText => $"resistance to {ResistanceTo} {Comparison} {Value}";
    }
}