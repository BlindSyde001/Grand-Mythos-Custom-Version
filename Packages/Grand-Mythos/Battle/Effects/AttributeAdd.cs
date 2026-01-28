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
        public static event Delegate? OnApplied;

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
            Formulas.GetCritModifiersBasedOnLuck(context.Profile.EffectiveStats.Luck, out var luckBasedChance, out var luckBasedMult);
            float critChanceTotal = CanCrit ? AdditionalCritChance + luckBasedChance : 0f;
            float critDamageMultiplier = AdditionalCritMultiplier + luckBasedMult;
            float sourceFlowScaler = SingletonManager.Instance.Formulas.SourceFlowScaler;
            float targetFlowScaler = SingletonManager.Instance.Formulas.TargetFlowScaler;
            foreach (var target in targets)
            {
                ComputableDamageScaling damageScaling; 
                damageScaling.Attribute = Attribute;
                damageScaling.BaseValue = Amount;
                damageScaling.CritChanceTotal = critChanceTotal;
                damageScaling.CritDeltaMultiplier = critDamageMultiplier;
                damageScaling.Scaling = Scaling;
                damageScaling.VarianceBase = Variance;
                damageScaling.VarianceRolled = context.Random.NextInt(-Variance, Variance+1);
                damageScaling.CritChanceRolled = context.Random.NextFloat(0f, 100f);
                damageScaling.SourceAttackStat = context.Profile.EffectiveStats.Attack;
                damageScaling.SourceMagicAttackStat = context.Profile.EffectiveStats.MagAttack;
                damageScaling.Element = Element;
                damageScaling.ResistanceFire = target.Profile.ResistanceFire;
                damageScaling.ResistanceIce = target.Profile.ResistanceIce;
                damageScaling.ResistanceLightning = target.Profile.ResistanceLightning;
                damageScaling.ResistanceWater = target.Profile.ResistanceWater;
                damageScaling.Missed = false;

                int initialAttributeValue = target.Profile.GetAttribute(Attribute);
                int maxAttributeValue = target.Profile.GetAttributeMax(Attribute);
                int currentValue = initialAttributeValue;

                foreach (var modifierOfSource in context.Profile.Modifiers)
                    modifierOfSource.Modifier.ModifyOutgoingDelta(context, target, ref damageScaling);

                foreach (var modifierOfTarget in target.Profile.Modifiers)
                    modifierOfTarget.Modifier.ModifyIncomingDelta(context, target, ref damageScaling);

                int initialValue = currentValue;
                damageScaling.ApplyDelta(ref currentValue);

                float delta = (float)(currentValue - initialValue) / maxAttributeValue;
                if (delta < 0)
                {
                    if (context.Profile.InFlowState == false)
                    {
                        context.Profile.CurrentFlow += -delta * sourceFlowScaler;
                        if (context.Profile.CurrentFlow >= 100f)
                        {
                            context.Profile.InFlowState = true;
                            context.Profile.CurrentFlow = 100f;
                        }
                    }
                    if (target.Profile.InFlowState == false)
                    {
                        target.Profile.CurrentFlow += -delta * targetFlowScaler;
                        if (target.Profile.CurrentFlow >= 100f)
                        {
                            target.Profile.InFlowState = true;
                            target.Profile.CurrentFlow = 100f;
                        }
                    }
                }

                target.Profile.SetAttribute(Attribute, currentValue);
                OnApplied?.Invoke(target, initialAttributeValue, damageScaling);
            }
        }

        public string UIDisplayText => $"{Attribute} += {Amount} {Scaling} {Element}";
    }
}