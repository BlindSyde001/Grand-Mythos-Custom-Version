using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Effects
{
    [Serializable]
    public class AttributeAdd : IEffect
    {
        public delegate void Delegate(BattleCharacterController target, Attribute attribute, int delta);
        public static event Delegate OnApplied;

        [HorizontalGroup, HideLabel, SuffixLabel("+=")]
        public Attribute Attribute = Attribute.Health;
        [HorizontalGroup, HideLabel, SuffixLabel("+-")]
        public int Amount = 10;

        [HorizontalGroup, HideLabel]
        public int Variance = 5;

        [HorizontalGroup, HideLabel]
        public ScalingType Scaling = ScalingType.Flat;
        [HorizontalGroup, HideLabel]
        public Element Element = Element.Neutral;

        [HorizontalGroup("Crit")]
        public bool CanCrit = true;
        [Range(0f, 100f), SuffixLabel("% added chance"), HideLabel, HorizontalGroup("Crit"), FormerlySerializedAs("CritChance")]
        public float AdditionalCritChance = 0f;
        [HorizontalGroup("Crit"), SuffixLabel("x added crit damage"), HideLabel, FormerlySerializedAs("CritMultiplier")]
        public float AdditionalCritMultiplier = 2.5f;

        public void Apply(BattleCharacterController[] targets, EvaluationContext context)
        {
            CriticalFormula.GetCritModifiersBasedOnLuck(context.Profile.EffectiveStats.Luck, out var luckBasedChance, out var luckBasedMult);
            float critChanceTotal = CanCrit ? AdditionalCritChance + luckBasedChance : 0f;
            float critDamageMultiplier = AdditionalCritMultiplier + luckBasedMult;
            foreach (var target in targets)
            {
                float delta = Amount + UnityEngine.Random.Range(-Variance, Variance+1);
                delta *= UnityEngine.Random.Range(0f, 100f) < critChanceTotal ? critDamageMultiplier : 1f;
                delta = Scaling switch
                {
                    ScalingType.Flat => delta,
                    ScalingType.Physical => delta * context.Profile.EffectiveStats.Attack,
                    ScalingType.Magical => delta * context.Profile.EffectiveStats.MagAttack,
                    _ => throw new ArgumentOutOfRangeException(Scaling.ToString())
                };

                var resistance = Element switch
                {
                    Element.Neutral => ElementalResistance.Neutral,
                    Element.Fire => target.Profile.ResistanceFire,
                    Element.Ice => target.Profile.ResistanceIce,
                    Element.Lighting => target.Profile.ResistanceLightning,
                    Element.Water => target.Profile.ResistanceWater,
                    _ => throw new ArgumentOutOfRangeException()
                };

                delta *= (float)resistance / 100f;

                int currentValue = target.Profile.GetAttribute(Attribute);
                int newValue = Mathf.RoundToInt(currentValue + delta);
                target.Profile.SetAttribute(Attribute, newValue);
                OnApplied?.Invoke(target, Attribute, newValue - currentValue);
            }
        }

        public string UIDisplayText => $"{Attribute} += {Amount} {Scaling} {Element}";

        public enum ScalingType
        {
            Flat,
            Physical,
            Magical,
        }
    }
}