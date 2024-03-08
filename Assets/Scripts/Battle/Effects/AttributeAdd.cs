using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Effects
{
    [Serializable]
    public class AttributeAdd : IEffect
    {
        [HorizontalGroup, HideLabel, SuffixLabel("+=")]
        public Attribute Attribute = Attribute.Health;
        [HorizontalGroup, HideLabel]
        public int Amount = 10;
        [HorizontalGroup, HideLabel]
        public ScalingType Scaling = ScalingType.Flat;

        public int Variance = 5;

        [Range(0f, 100f), SuffixLabel("%"), HorizontalGroup("Crit")]
        public float CritChance = 0f;
        [HorizontalGroup("Crit")]
        public float CritMultiplier = 2.5f;

        public IEnumerable Apply(BattleCharacterController[] targets, EvaluationContext context)
        {
            foreach (var target in targets)
            {
                float amount = Amount + UnityEngine.Random.Range(-Variance, Variance+1);
                amount *= UnityEngine.Random.Range(0f, 100f) < CritChance ? CritMultiplier : 1f;
                amount = Scaling switch
                {
                    ScalingType.Flat => amount,
                    ScalingType.Physical => amount * context.Profile.EffectiveStats.Attack,
                    ScalingType.Magical => amount * context.Profile.EffectiveStats.MagAttack,
                    _ => throw new ArgumentOutOfRangeException(Scaling.ToString())
                };

                int value = target.Profile.GetAttribute(Attribute);
                target.Profile.SetAttribute(Attribute, Mathf.RoundToInt(value + amount));
            }

            yield break;
        }

        public string UIDisplayText => $"{Attribute} += {Amount} {Scaling}";

        public enum ScalingType
        {
            Flat,
            Physical,
            Magical,
        }
    }
}