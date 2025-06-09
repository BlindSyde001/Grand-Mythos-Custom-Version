using System;
using QTE;

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

    public QTEResult QTEScaler;

    public int SourceAttackStat;
    public int SourceMagicAttackStat;

    public Element Element;
    public ElementalResistance ResistanceFire;
    public ElementalResistance ResistanceIce;
    public ElementalResistance ResistanceLightning;
    public ElementalResistance ResistanceWater;

    public bool Missed;

    public void ApplyDelta(ref int currentValue)
    {
        if (Missed)
            return;

        double delta = BaseValue + VarianceRolled;
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

        delta *= (double)resistance / 100.0;
        delta *= QTEScaler switch
        {
            QTEResult.Failure => 0,
            QTEResult.Correct => 1,
            QTEResult.Success => 2,
            _ => throw new ArgumentOutOfRangeException()
        };
        currentValue = currentValue + (int)delta; 
    }

    public enum ScalingType
    {
        Flat,
        Physical,
        Magical,
    }
}