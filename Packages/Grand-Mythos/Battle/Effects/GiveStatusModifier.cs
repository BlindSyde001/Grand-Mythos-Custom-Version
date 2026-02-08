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
        
        public void Apply(CharacterTemplate[] targets, EvaluationContext context)
        {
            foreach (var target in targets)
            {
                if (context.Random.NextFloat(0,100) <= Chance)
                    target.Modifiers.Add(new AppliedModifier(context.CombatTimestamp, Modifier, context.Profile));
            }
        }

        public string UIDisplayText => $"{Chance}% chance to apply {(Modifier == null! ? "?" : Modifier.name)}";
    }
}