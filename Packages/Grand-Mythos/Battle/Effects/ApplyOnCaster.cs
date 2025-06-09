using System;
using QTE;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Effects
{
    [Serializable]
    public class ApplyOnCaster : IEffect
    {
        [Space]
        [ListDrawerSettings(ShowFoldout = false)]
        [LabelText(@"@""Effects:   "" + this.UIDisplaySubText")]
        [SerializeReference]
        public IEffect[] Effects = Array.Empty<IEffect>();

        public void Apply(BattleCharacterController[] targets, QTEResult result, EvaluationContext context)
        {
            var casterTarget = new BattleCharacterController[]{ context.Controller };
            foreach (var effect in Effects)
                effect.Apply(casterTarget, result, context);
        }

        public string UIDisplayText => $"{Effects.UIDisplayText()} on Caster";

        public string UIDisplaySubText => Effects.UIDisplayText();
    }
}