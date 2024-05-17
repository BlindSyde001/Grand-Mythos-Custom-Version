using System;
using Conditions;
using Nodalog;
using Sirenix.OdinInspector;

namespace Interactables.Conditions
{
    [Serializable]
    public class TallyIs : ICondition
    {
        [HorizontalGroup, HideLabel, Required]
        public Tally Tally;

        [HorizontalGroup, HideLabel]
        public ComparisonType Comparison = ComparisonType.EqualTo;

        [HorizontalGroup, HideLabel, SuffixLabel("?")]
        public int Value = 1;

        public bool Evaluate() => Comparison switch
        {
            ComparisonType.LesserThan => Tally.Amount < Value,
            ComparisonType.LessOrEqualTo => Tally.Amount <= Value,
            ComparisonType.EqualTo => Tally.Amount == Value,
            ComparisonType.GreaterOrEqualTo => Tally.Amount >= Value,
            ComparisonType.GreaterThan => Tally.Amount > Value,
            ComparisonType.NotEqualTo => Tally.Amount != Value,
            _ => throw new ArgumentOutOfRangeException()
        };

        public bool IsValid(out string error)
        {
            if (Tally == null)
            {
                error = $"{nameof(Tally)} is null";
                return false;
            }

            error = "";
            return true;
        }
    }
}