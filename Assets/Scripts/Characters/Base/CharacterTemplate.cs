using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public struct Stats
{
    [HorizontalGroup("POINTS")]
    [GUIColor(0.5f, 1f, 0.5f)]
    public int HP;

    [HorizontalGroup("POINTS")]
    [GUIColor(0.5f, 0.5f, 0.9f)]
    public int MP;

    [HorizontalGroup("ATTACKS")]
    [GUIColor(1f, 0.5f, 0.5f)]
    public int Attack;

    [HorizontalGroup("ATTACKS")]
    [GUIColor(1f, 0.5f, 0.5f)]
    public int MagAttack;

    [HorizontalGroup("DEFENSE")]
    [GUIColor(0.5f, 0.8f, 0.8f)]
    public int Defense;

    [HorizontalGroup("DEFENSE")]
    [GUIColor(0.5f, 0.8f, 0.8f)]
    public int MagDefense;

    public int Speed;
}

public class CharacterTemplate : MonoBehaviour
{
    [Required]
    [PropertyOrder(0)]
    public Team Team;

    [SerializeField]
    [PreviewField(100)]
    [PropertyOrder(0)]
    [HideLabel]
    public Sprite charPortrait;

    [SerializeField]
    [PreviewField(100)]
    [PropertyOrder(0)]
    [HideLabel]
    public GameObject _CharacterModel;

    [TitleGroup("CHARACTER ATTRIBUTES")]
    [HorizontalGroup("CHARACTER ATTRIBUTES/Split")]
    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Left")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Left/STATS")]
    [HorizontalGroup("CHARACTER ATTRIBUTES/Split/Left/STATS/POINTS")]
    [GUIColor(0f, 1f, 0.3f)]
    public int CurrentHP;

    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Left")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Left/STATS")]
    [HorizontalGroup("CHARACTER ATTRIBUTES/Split/Left/STATS/POINTS")]
    [GUIColor(0.5f, 0.5f, 0.9f)]
    public int CurrentMP;

    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Left")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Left/STATS")]
    [OnValueChanged(nameof(UpdateDummyStats))]
    public Stats BaseStats = new(){ HP = 22, MP = 10, Attack = 8, MagAttack = 8, Defense = 8, MagDefense = 8, Speed = 20 };

    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Left")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Left/STATS")]
    [SerializeField]
    [ReadOnly]
    private Stats _effectiveStatsPreview;

    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Left")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Left/STATS")]
    public GrowthRate GrowthRate;

    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Left")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Left/STATS")]
    [OnValueChanged(nameof(UpdateLevelFromExperience))]
    public int Experience;

    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Left")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Left/STATS")]
    [OnValueChanged(nameof(UpdateExperienceFromLevel))]
    public int StartingLevel = 1;

    /// <summary>
    /// Stats after all mods, like level, equipment and status effects, have been applied
    /// </summary>
    public virtual Stats EffectiveStats => new Stats
    {
        HP = (int)(BaseStats.HP + BaseStats.HP * GetGrowthRateMultiplier(GrowthRate) * StartingLevel),
        MP = (int)(BaseStats.MP + BaseStats.MP * GetGrowthRateMultiplier(GrowthRate) * StartingLevel),
        Attack = (int)(BaseStats.Attack + BaseStats.Attack * GetGrowthRateMultiplier(GrowthRate) * StartingLevel),
        MagAttack = (int)(BaseStats.MagAttack + BaseStats.MagAttack * GetGrowthRateMultiplier(GrowthRate) * StartingLevel),
        Defense = (int)(BaseStats.Defense + BaseStats.Defense * GetGrowthRateMultiplier(GrowthRate) * StartingLevel),
        MagDefense = (int)(BaseStats.MagDefense + BaseStats.MagDefense * GetGrowthRateMultiplier(GrowthRate) * StartingLevel),
        Speed = BaseStats.Speed,
    };

    [BoxGroup("ELEMENTAL RESISTANCES")]
    [PropertyRange(-100, 100)]
    [PropertyOrder(1.1f)]
    public int _AffinityFIRE;

    [BoxGroup("ELEMENTAL RESISTANCES")]
    [PropertyRange(-100, 100)]
    [PropertyOrder(1.1f)]
    public int _AffinityICE;

    [BoxGroup("ELEMENTAL RESISTANCES")]
    [PropertyRange(-100, 100)]
    [PropertyOrder(1.1f)]
    public int _AffinityLIGHTNING;

    [BoxGroup("ELEMENTAL RESISTANCES")]
    [PropertyRange(-100, 100)]
    [PropertyOrder(1.1f)]
    public int _AffinityWATER;

    [BoxGroup("STATUS RESISTANCES")]
    [PropertyRange(-100, 100)]
    [PropertyOrder(1.2f)]
    public int _ResistBLIND;

    [BoxGroup("STATUS RESISTANCES")]
    [PropertyRange(-100, 100)]
    [PropertyOrder(1.2f)]
    public int _ResistSILENCE;

    [BoxGroup("STATUS RESISTANCES")]
    [PropertyRange(-100, 100)]
    [PropertyOrder(1.2f)]
    public int _ResistFUROR;

    [BoxGroup("STATUS RESISTANCES")]
    [PropertyRange(-100, 100)]
    [PropertyOrder(1.2f)]
    public int _ResistPARALYSIS;

    [BoxGroup("STATUS RESISTANCES")]
    [PropertyRange(-100, 100)]
    [PropertyOrder(1.2f)]
    public int _ResistPHYSICAL;

    [BoxGroup("STATUS RESISTANCES")]
    [PropertyRange(-100, 100)]
    [PropertyOrder(1.2f)]
    public int _ResistMAGICAL;

    [BoxGroup("AFFLICTION STATUS")]
    [PropertyOrder(1.3f)]
    public bool _IsBlinded;

    [BoxGroup("AFFLICTION STATUS")]
    [PropertyOrder(1.3f)]
    public bool _IsSilenced;

    [BoxGroup("AFFLICTION STATUS")]
    [PropertyOrder(1.3f)]
    public bool _IsFurored;

    [BoxGroup("AFFLICTION STATUS")]
    [PropertyOrder(1.3f)]
    public bool _IsParalysed;

    [BoxGroup("AFFLICTION STATUS")]
    [PropertyOrder(1.3f)]
    public bool _IsPhysDown;

    [BoxGroup("AFFLICTION STATUS")]
    [PropertyOrder(1.3f)]
    public bool _IsMagDown;

    public float ActionRechargeSpeed => EffectiveStats.Speed;

    [BoxGroup("ATB")]
    [PropertyRange(0, 100)]
    [PropertyOrder(4)]
    public float ActionsCharged;

    [BoxGroup("ATB")]
    [PropertyOrder(4)]
    public uint ActionChargeMax = 4;

    [BoxGroup("INVENTORY")]
    [SerializeReference]
    [Required]
    [PropertyOrder(5)]
    public IInventory Inventory = new InlineInventory();

    [BoxGroup("SKILLS")]
    [SerializeField]
    [Required]
    [PropertyOrder(6)]
    public Skill BasicAttack;

    [BoxGroup("SKILLS")]
    [SerializeField]
    [PropertyOrder(6)]
    public SkillTree SkillTree;

    [BoxGroup("SKILLS")]
    [SerializeField]
    [PropertyOrder(6)]
    public SerializableHashSet<Skill> Skills;

    [Required]
    [PropertyOrder(7)]
    public Tactics[] Tactics = Array.Empty<Tactics>();

    /// <summary>
    /// How much experience points this unit gives on death
    /// </summary>
    [BoxGroup("DROP")]
    public int experiencePool;
    /// <summary>
    /// How many Credits this unit gives on death
    /// </summary>
    [BoxGroup("DROP")]
    public int creditPool;

    /// <summary>
    /// Loot that the enemy drops
    /// </summary>
    [BoxGroup("DROP")]
    public List<ItemCapsule> DropItems;
    [BoxGroup("DROP")]
    public List<float> DropRate;



    [NonSerialized]
    public EvaluationContext Context = new();

    void UpdateDummyStats()
    {
        RegenHealthAndMana();
        _effectiveStatsPreview = EffectiveStats;
    }

    void UpdateExperienceFromLevel()
    {
        UpdateDummyStats();
        Experience = GetAmountOfXPForLevel(StartingLevel);
    }

    void UpdateLevelFromExperience()
    {
        UpdateDummyStats();
        StartingLevel = GetAmountOfLevelForXP(Experience);
    }

    public static int GetAmountOfXPForLevel(int level)
    {
        if (level <= 0)
            return 0;
        level -= 1;
        double temp = level;
        temp *= 5d;
        temp = Math.Pow(temp, 2.3d);
        return (int)Math.Floor(temp);
    }

    public static int GetAmountOfLevelForXP(int xp)
    {
        if (xp <= 0)
            return 0;
        double temp = Math.Pow(xp+1, 1d/2.3d);
        temp /= 5d;
        return ((int)temp) + 1;
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

    public CharacterTemplate()
    {
        Context.Source = this;
    }

    protected virtual void Awake()
    {
        RegenHealthAndMana();
    }

    protected void RegenHealthAndMana()
    {
        CurrentHP = EffectiveStats.HP;
        CurrentMP = EffectiveStats.MP;
    }

    public bool IsHostileTo(CharacterTemplate character)
    {
        return Team.Allies.Contains(character.Team) == false;
    }

    public int GetAttribute(Attribute attribute) => attribute switch
    {
        Attribute.Health => CurrentHP,
        Attribute.Mana => CurrentMP,
        Attribute.HealthPercent => Mathf.CeilToInt((float)CurrentHP / EffectiveStats.HP * 100f),
        Attribute.ManaPercent => Mathf.CeilToInt((float)CurrentMP / EffectiveStats.MP * 100f),
        Attribute.MaxHealth => EffectiveStats.HP,
        Attribute.MaxMana => EffectiveStats.MP,
        Attribute.Attack => EffectiveStats.Attack,
        Attribute.MagicAttack => EffectiveStats.MagAttack,
        Attribute.Defense => EffectiveStats.Defense,
        Attribute.MagicDefense => EffectiveStats.MagDefense,
        Attribute.Speed => EffectiveStats.Speed,
        _ => throw new ArgumentOutOfRangeException(nameof(attribute), attribute, null)
    };

    public void SetAttribute(Attribute attribute, int value)
    {
        switch(attribute)
        {
            case Attribute.Health: CurrentHP = Mathf.Clamp(value, 0, EffectiveStats.HP); break;
            case Attribute.Mana: CurrentMP = Mathf.Clamp(value, 0, EffectiveStats.MP); break;
            case Attribute.HealthPercent: CurrentHP = Mathf.CeilToInt(value / 100f * EffectiveStats.HP); break;
            case Attribute.ManaPercent: CurrentMP = Mathf.CeilToInt(value / 100f * EffectiveStats.MP); break;
            default: throw new ArgumentOutOfRangeException(nameof(attribute), attribute, null);
        }
    }

    public bool HasStatus(Status status) => status switch
    {
        Status.Blind => _IsBlinded,
        Status.Silenced => _IsSilenced,
        Status.Furored => _IsFurored,
        Status.Paralysed => _IsParalysed,
        Status.PhysDown => _IsPhysDown,
        Status.MagDown => _IsMagDown,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    public int GetStatusResistance(Status status) => status switch
    {
        Status.Blind => _ResistBLIND,
        Status.Silenced => _ResistSILENCE,
        Status.Furored => _ResistFUROR,
        Status.Paralysed => _ResistPARALYSIS,
        Status.PhysDown => _ResistPHYSICAL,
        Status.MagDown => _ResistMAGICAL,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };
}


public enum Attribute
{
    Health,
    Mana,
    HealthPercent,
    ManaPercent,
    MaxHealth,
    MaxMana,
    Attack,
    MagicAttack,
    Defense,
    MagicDefense,
    Speed,
}

public enum Status
{
    Blind,
    Silenced,
    Furored,
    Paralysed,
    PhysDown,
    MagDown,
}


public enum GrowthRate
{
    Fixed,
    Weak,
    Average,
    Strong,
    Hyper,
}