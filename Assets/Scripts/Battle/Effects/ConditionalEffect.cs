using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Effects
{
    [Serializable]
    public class ConditionalEffect : IEffect
    {
        [SerializeReference, Required]
        public Condition Condition;
        [LabelText(@"@""Effects:   "" + this.UIDisplaySubText")]
        [SerializeReference]
        public IEffect[] Effects = Array.Empty<IEffect>();

        public IEnumerable Apply(TargetCollection targets, EvaluationContext context)
        {
            var newTargets = targets;
            Condition.Filter(ref newTargets, context);
            foreach (var effect in Effects)
            {
                foreach (var yield in effect.Apply(targets, context))
                    yield return yield;
            }
        }

        public string UIDisplayText => $"when {Condition?.UIDisplayText} do {Effects.UIDisplayText()}";

        public string UIDisplaySubText => Effects.UIDisplayText();
    }
}