using System;
using System.Diagnostics.CodeAnalysis;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Conditions
{
    [Serializable]
    public class RandomPerRound : Condition
    {
        [Range(0f, 100f), HideLabel, SuffixLabel("%")]
        public float Chance = 50f;
        protected override void FilterInner(ref TargetCollection targets, EvaluationContext context)
        {
            Random random = new Random((uint)GetHashCode() + context.Round + context.CombatSeed);
            if (random.NextFloat(0f, 100f) >= Chance)
                targets.Empty();
        }

        public override bool IsValid([MaybeNullWhen(true)] out string error)
        {
            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context) { }

        public override string UIDisplayText => $"{Chance}% chance";
    }
}