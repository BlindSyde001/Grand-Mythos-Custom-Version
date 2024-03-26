using System;
using System.Linq;
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

        public void Apply(BattleCharacterController[] targets, EvaluationContext context)
        {
            var collection = new TargetCollection(targets.ToList());
            Condition.Filter(ref collection, context);
            targets = collection.ToArray();
            foreach (var effect in Effects)
                effect.Apply(targets, context);
        }

        public string UIDisplayText => $"when {Condition?.UIDisplayText} do {Effects.UIDisplayText()}";

        public string UIDisplaySubText => Effects.UIDisplayText();
    }
}