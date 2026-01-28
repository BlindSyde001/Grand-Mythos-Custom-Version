using System;
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

        public void Apply(BattleCharacterController[] targets, EvaluationContext context)
        {
            var casterTarget = new[]{ context.Controller };
            foreach (var effect in Effects)
                effect.Apply(casterTarget, context);
        }

        public string UIDisplayText => $"{Effects.UIDisplayText()} on Caster";

        public string UIDisplaySubText => Effects.UIDisplayText();
    }
}