using System;
using Random = Unity.Mathematics.Random;

namespace Conditions
{
    [Serializable]
    public class TargetRandom : Condition
    {
        protected override void FilterInner(ref TargetCollection targets, EvaluationContext context)
        {
            Random random = new Random((uint)GetHashCode() + context.Round + context.CombatSeed);
            int selection = random.NextInt(0, targets.CountSlow());
            for (int i = -1, c = 0; targets.TryGetNext(ref i, out _); c++)
            {
                if (c != selection)
                    targets.RemoveAt(i);
            }
        }

        public override bool IsValid(out string error)
        {
            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context) { }

        public override string UIDisplayText => $"is randomly selected (1 unit max)";
    }
}