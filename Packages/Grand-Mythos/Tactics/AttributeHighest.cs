using System;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

namespace Conditions
{
    [Serializable]
    public class AttributeHighest : Condition
    {
        [InfoBox(AttributeLowest.InfoBoxText, InfoMessageType.Warning), LabelText("Pick highest")]
        public Attribute TargetAttribute;

        protected override void FilterInner(ref TargetCollection targets, EvaluationContext context)
        {
            double highestStat = double.NegativeInfinity;
            int highestTarget = -1;
            for (int i = -1; targets.TryGetNext(ref i, out var target);)
            {
                var thisTargetStat = target.Profile.GetAttribute(TargetAttribute);
                if (thisTargetStat > highestStat)
                {
                    highestStat = thisTargetStat;
                    highestTarget = i;
                }
            }

            for (int i = -1; targets.TryGetNext(ref i, out _);)
            {
                if (i != highestTarget)
                    targets.RemoveAt(i);
            }
        }

        public override bool IsValid(out string error)
        {
            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context){}

        public override string UIDisplayText => $"with highest {TargetAttribute}";
    }
}