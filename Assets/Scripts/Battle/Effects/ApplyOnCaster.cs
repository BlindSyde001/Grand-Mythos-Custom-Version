using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Effects
{
    [Serializable]
    public class ApplyOnCaster : IEffect
    {
        [ListDrawerSettings(ShowFoldout = false)]
        [LabelText(@"@""Effects:   "" + this.UIDisplaySubText")]
        [SerializeReference]
        public IEffect[] Effects = Array.Empty<IEffect>();

        public void Apply(BattleCharacterController[] targets, EvaluationContext context)
        {
            var casterTarget = new BattleCharacterController[]{ context.Controller };
            foreach (var effect in Effects)
                effect.Apply(casterTarget, context);
        }

        public string UIDisplayText => $"Caster: {Effects.UIDisplayText()}";

        public string UIDisplaySubText => Effects.UIDisplayText();
    }
}