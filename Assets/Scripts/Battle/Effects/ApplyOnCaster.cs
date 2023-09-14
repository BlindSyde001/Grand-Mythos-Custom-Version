using System;
using System.Collections;
using System.Collections.Generic;
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

        public IEnumerable Apply(TargetCollection targets, EvaluationContext context)
        {
            var casterTarget = new List<CharacterTemplate>();
            casterTarget.Add(context.Source);
            var casterTargetCollection = new TargetCollection(casterTarget);
            foreach (var effect in Effects)
            {
                foreach (var yield in effect.Apply(casterTargetCollection, context))
                    yield return yield;
            }
        }

        public string UIDisplayText => $"Caster: {Effects.UIDisplayText()}";

        public string UIDisplaySubText => Effects.UIDisplayText();
    }
}