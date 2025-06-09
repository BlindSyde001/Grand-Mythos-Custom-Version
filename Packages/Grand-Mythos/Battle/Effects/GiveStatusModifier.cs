using System;
using Characters;
using QTE;
using Sirenix.OdinInspector;
using StatusHandler;
using UnityEngine;

namespace Effects
{
    [Serializable]
    public class GiveStatusModifier : IEffect
    {
        [Required] public StatusModifier Modifier;
        [Range(0, 100), SuffixLabel("%")] public float Chance = 50f;
        
        public void Apply(BattleCharacterController[] targets, QTEResult result, EvaluationContext context)
        {
            foreach (var target in targets)
            {
                if (context.Random.NextFloat(0,100) <= Chance && result is QTEResult.Correct or QTEResult.Success)
                    target.Profile.Modifiers.Add(new AppliedModifier(target.Context, Modifier, context.Profile));
            }
        }

        public string UIDisplayText => $"{Chance}% chance to apply {(Modifier == null ? "?" : Modifier.name)}";
    }
}