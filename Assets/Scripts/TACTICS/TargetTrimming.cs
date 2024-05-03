using System;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

namespace Conditions
{
    [Serializable]
    public class TargetTrimming : Condition
    {
        [FormerlySerializedAs("Amount"), LabelText("Target Amount Max")]
        public int Max = 1;

        protected override void FilterInner(ref TargetCollection targets, EvaluationContext context)
        {
            int left = Max;
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

        public override string UIDisplayText => $"Keep {Max} target maximum";
    }
}