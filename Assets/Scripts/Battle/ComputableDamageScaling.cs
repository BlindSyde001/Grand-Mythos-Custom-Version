using System;
using UnityEngine;

public struct ComputableDamageScaling
{
    public Attribute Attribute;
    public float BaseValue;
    public float CritChanceTotal;
    public float CritDeltaMultiplier;
    public ScalingType Scaling;
    public int VarianceBase;
    public int VarianceRolled;
    public float CritChanceRolled;

    public int SourceAttackStat;
    public int SourceMagicAttackStat;

    public Element Element;
    public ElementalResistance ResistanceFire;
    public ElementalResistance ResistanceIce;
    public ElementalResistance ResistanceLightning;
    public ElementalResistance ResistanceWater;

    public void ApplyDelta(ref int currentValue)
    {
        float delta = BaseValue + VarianceRolled;
        delta *= CritChanceRolled < CritChanceTotal ? CritDeltaMultiplier : 1f;
        delta = Scaling switch
        {
            ScalingType.Flat => delta,
            ScalingType.Physical => delta * SourceAttackStat,
            ScalingType.Magical => delta * SourceMagicAttackStat,
            _ => throw new ArgumentOutOfRangeException(Scaling.ToString())
        };

        var resistance = Element switch
        {
            Element.Neutral => ElementalResistance.Neutral,
            Element.Fire => ResistanceFire,
            Element.Ice => ResistanceIce,
            Element.Lighting => ResistanceLightning,
            Element.Water => ResistanceWater,
            _ => throw new ArgumentOutOfRangeException()
        };

        delta *= (float)resistance / 100f;
        currentValue = Mathf.RoundToInt(currentValue + delta);
    }

    public enum ScalingType
    {
        Flat,
        Physical,
        Magical,
    }
}