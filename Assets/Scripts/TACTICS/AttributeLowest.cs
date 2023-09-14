using System;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

namespace Conditions
{
    [Serializable]
    public class AttributeLowest : Condition
    {
        public const string InfoBoxText = "This condition always removes all targets from the selection but one,\n" +
                                          "in most cases, like if you want to use it as a filter to target allies or hostiles,\n" +
                                          "you must place this one at the rightmost position in your 'AND'";

        [InfoBox(InfoBoxText, InfoMessageType.Warning), LabelText("Pick lowest")]
        public Attribute TargetAttribute;

        protected override void FilterInner(ref TargetCollection targets, EvaluationContext context)
        {
            double lowestValue = double.PositiveInfinity;
            int lowestTarget = -1;
            for (int i = -1; targets.TryGetNext(ref i, out var target);)
            {
                var thisTargetStat = target.GetAttribute(TargetAttribute);
                if (thisTargetStat < lowestValue)
                {
                    lowestValue = thisTargetStat;
                    lowestTarget = i;
                }
            }

            for (int i = -1; targets.TryGetNext(ref i, out _);)
            {
                if (i != lowestTarget)
                    targets.RemoveAt(i);
            }
        }

        public override bool IsValid(out string error)
        {
            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context){}

        public override string UIDisplayText => $"pick target with lowest {TargetAttribute}";
    }
}