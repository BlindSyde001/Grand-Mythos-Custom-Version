using System;
using Sirenix.OdinInspector;

namespace Conditions
{
    [Serializable]
    public class TargetTrimming : Condition
    {
        [HideLabel, SuffixLabel("target max.")]
        public int Amount = 1;

        protected override void FilterInner(ref TargetCollection targets, EvaluationContext context)
        {
            int left = Amount;
            for (int i = -1; targets.TryGetNext(ref i, out _); )
            {
                if (left != 0)
                {
                    left--;
                }
                else
                {
                    targets.RemoveAt(i);
                }
            }
        }

        public override bool IsValid(out string error)
        {
            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context){}

        public override string UIDisplayText => $"Keep {Amount} target maximum";
    }
}