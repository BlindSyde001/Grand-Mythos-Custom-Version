using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Effects
{
    [Serializable]
    public class AttributeAdd : IEffect
    {
        public delegate void Delegate(BattleCharacterController target, int initialAttributeValue, ComputableDamageScaling delta);
        public static event Delegate OnApplied;

        [HorizontalGroup, HideLabel, SuffixLabel("+=")]
        public Attribute Attribute = Attribute.Health;
        [HorizontalGroup, HideLabel, SuffixLabel("+-")]
        public int Amount = 10;

        [HorizontalGroup, HideLabel]
        public int Variance = 5;

        [HorizontalGroup, HideLabel]
        public ComputableDamageScaling.ScalingType Scaling = ComputableDamageScaling.ScalingType.Flat;
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
                var damageScaling = new ComputableDamageScaling
                {
                    Attribute = Attribute,
                    BaseValue = Amount,
                    CritChanceTotal = critChanceTotal,
                    CritDeltaMultiplier = critDamageMultiplier,
                    Scaling = Scaling,
                    VarianceBase = Variance,
                    VarianceRolled = UnityEngine.Random.Range(-Variance, Variance+1),
                    CritChanceRolled = UnityEngine.Random.Range(0f, 100f),
                    SourceAttackStat = context.Profile.EffectiveStats.Attack,
                    SourceMagicAttackStat = context.Profile.EffectiveStats.MagAttack,
                    Element = Element,
                    ResistanceFire = target.Profile.ResistanceFire,
                    ResistanceIce = target.Profile.ResistanceIce,
                    ResistanceLightning = target.Profile.ResistanceLightning,
                    ResistanceWater = target.Profile.ResistanceWater,
                };

                int initialAttributeValue = target.Profile.GetAttribute(Attribute);
                int currentValue = initialAttributeValue;

                foreach (var modifierOfSource in context.Profile.Modifiers)
                    modifierOfSource.ModifyOutgoingDelta(context, target, ref damageScaling);

                foreach (var modifierOfTarget in target.Profile.Modifiers)
                    modifierOfTarget.ModifyIncomingDelta(context, target, ref damageScaling);

                damageScaling.ApplyDelta(ref currentValue);
                target.Profile.SetAttribute(Attribute, currentValue);
                OnApplied?.Invoke(target, initialAttributeValue, damageScaling);
            }
        }

        public string UIDisplayText => $"{Attribute} += {Amount} {Scaling} {Element}";
    }
}