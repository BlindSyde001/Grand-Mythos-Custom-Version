using System;
using Sirenix.OdinInspector;

[Serializable]
public struct StatGrowth
{
    [HorizontalGroup("POINTS"), GUIColor(0.5f, 1f, 0.5f)]
    public GrowthRate HP;

    [HorizontalGroup("POINTS"), GUIColor(0.5f, 0.5f, 0.9f)]
    public GrowthRate MP;

    [HorizontalGroup("ATTACKS"), GUIColor(1f, 0.5f, 0.5f)]
    public GrowthRate Attack;

    [HorizontalGroup("ATTACKS"), GUIColor(1f, 0.5f, 0.5f)]
    public GrowthRate MagAttack;

    [HorizontalGroup("DEFENSE"), GUIColor(0.5f, 0.8f, 0.8f)]
    public GrowthRate Defense;

    [HorizontalGroup("DEFENSE"), GUIColor(0.5f, 0.8f, 0.8f)]
    public GrowthRate MagDefense;

    [HorizontalGroup("MISC"), GUIColor(0.75f, 0.75f, 0.75f)]
    public GrowthRate Speed;

    [HorizontalGroup("MISC"), GUIColor(0.75f, 0.75f, 0.75f)]
    public GrowthRate Luck;

    public Stats ApplyGrowth(Stats baseStats, uint level)
    {
        return new()
        {
            HP = (int)(baseStats.HP + baseStats.HP * GetGrowthRateMultiplier(HP) * level),
            MP = (int)(baseStats.MP + baseStats.MP * GetGrowthRateMultiplier(MP) * level),
            Attack = (int)(baseStats.Attack + baseStats.Attack * GetGrowthRateMultiplier(Attack) * level),
            MagAttack = (int)(baseStats.MagAttack + baseStats.MagAttack * GetGrowthRateMultiplier(MagAttack) * level),
            Defense = (int)(baseStats.Defense + baseStats.Defense * GetGrowthRateMultiplier(Defense) * level),
            MagDefense = (int)(baseStats.MagDefense + baseStats.MagDefense * GetGrowthRateMultiplier(MagDefense) * level),
            Speed = (int)(baseStats.Speed + baseStats.Speed * GetGrowthRateMultiplier(Speed) * level),
            Luck = (int)(baseStats.Luck + baseStats.Luck * GetGrowthRateMultiplier(Luck) * level),
        };
    }

    public static float GetGrowthRateMultiplier(GrowthRate growthRate) => growthRate switch
    {
        GrowthRate.Average => 1.2f,
        GrowthRate.Strong => 1.3f,
        GrowthRate.Hyper => 1.5f,
        GrowthRate.Weak => 1.1f,
        GrowthRate.Fixed => 0f,
        _ => throw new ArgumentOutOfRangeException(nameof(growthRate), growthRate, null)
    };

    public enum GrowthRate
    {
        Fixed,
        Weak,
        Average,
        Strong,
        Hyper,
    }
}