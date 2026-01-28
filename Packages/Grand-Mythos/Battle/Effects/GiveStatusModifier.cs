using System;
using Characters;
using Sirenix.OdinInspector;
using StatusHandler;
using UnityEngine;

namespace Effects
{
    [Serializable]
    public class GiveStatusModifier : IEffect
    {
        public required StatusModifier Modifier;
        [Range(0, 100), SuffixLabel("%")] public float Chance = 50f;
        
        public void Apply(BattleCharacterController[] targets, EvaluationContext context)
        {
            foreach (var target in targets)
            {
                if (context.Random.NextFloat(0,100) <= Chance)
                    target.Profile.Modifiers.Add(new AppliedModifier(target.Context, Modifier, context.Profile));
            }
        }

        public string UIDisplayText => $"{Chance}% chance to apply {(Modifier == null! ? "?" : Modifier.name)}";
    }
}