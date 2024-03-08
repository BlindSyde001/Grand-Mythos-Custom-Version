using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

[Serializable]
public struct Stats
{
    [HorizontalGroup("POINTS"), GUIColor(0.5f, 1f, 0.5f)]
    public int HP;

    [HorizontalGroup("POINTS"), GUIColor(0.5f, 0.5f, 0.9f)]
    public int MP;

    [HorizontalGroup("ATTACKS"), GUIColor(1f, 0.5f, 0.5f)]
    public int Attack;

    [HorizontalGroup("ATTACKS"), GUIColor(1f, 0.5f, 0.5f)]
    public int MagAttack;

    [HorizontalGroup("DEFENSE"), GUIColor(0.5f, 0.8f, 0.8f)]
    public int Defense;

    [HorizontalGroup("DEFENSE"), GUIColor(0.5f, 0.8f, 0.8f)]
    public int MagDefense;

    public int Speed;
}

public class CharacterTemplate : MonoBehaviour
{
    [Required]
    public Team Team;

    [HorizontalGroup("ASSETS"), ValidateInput(nameof(HasButton), "Must have a BattleHeroModelController"), SerializeField, PreviewField(100)]
    public GameObject BattlePrefab;

    [HorizontalGroup("ASSETS"), SerializeField, PreviewField(100)]
    public Sprite Portrait;

    [BoxGroup("STATS"), HorizontalGroup("STATS/POINTS"), GUIColor(0f, 1f, 0.3f)]
    public int CurrentHP;

    [HorizontalGroup("STATS/POINTS"), GUIColor(0.5f, 0.5f, 0.9f)]
    public int CurrentMP;

    [BoxGroup("STATS"), OnValueChanged(nameof(UpdateDummyStats))]
    public Stats BaseStats = new(){ HP = 22, MP = 10, Attack = 8, MagAttack = 8, Defense = 8, MagDefense = 8, Speed = 20 };

    [BoxGroup("STATS"), SerializeField, ReadOnly]
    Stats _effectiveStatsPreview;

    [BoxGroup("STATS"), OnValueChanged(nameof(UpdateDummyStats))]
    public GrowthRate GrowthRate;

    [BoxGroup("STATS"), OnValueChanged(nameof(UpdateLevelFromExperience))]
    public int Experience;

    [FormerlySerializedAs("StartingLevel"), BoxGroup("STATS"), OnValueChanged(nameof(UpdateExperienceFromLevel))]
    public int Level = 1;

    [BoxGroup("ELEMENTAL RESISTANCES"), PropertyRange(-100, 100)]
    public int AffinityFIRE, AffinityICE, AffinityLIGHTNING, AffinityWATER;

    [HorizontalGroup("STATUS"), VerticalGroup("STATUS/Left"), BoxGroup("STATUS/Left/AFFLICTION STATUS")]
    public bool IsBlinded, IsSilenced, IsFurored, IsParalysed, IsPhysDown, IsMagDown;

    [VerticalGroup("STATUS/Right"), BoxGroup("STATUS/Right/AFFLICTION RESISTANCE"), PropertyRange(-100, 100)]
    public int ResistBLIND, ResistSILENCE, ResistFUROR, ResistPARALYSIS, ResistPHYSICAL, ResistMAGICAL;

    public float ActionRechargeSpeed => EffectiveStats.Speed;

    [BoxGroup("ATB"), PropertyRange(0, 100)]
    public float ActionsCharged;

    [BoxGroup("ATB")]
    public uint ActionChargeMax = 4;

    [BoxGroup("SKILLS"), Required]
    public Skill BasicAttack;

    [BoxGroup("SKILLS"), MaybeNull]
    public SkillTree SkillTree;

    [BoxGroup("SKILLS")]
    public SerializableHashSet<Skill> Skills;

    [BoxGroup("TACTICS"), Required, ListDrawerSettings(ShowFoldout = false)]
    public Tactics[] Tactics = Array.Empty<Tactics>();

    [BoxGroup("ANIMATIONS")]
    [ValidateInput(nameof(ValidateActionAnimation)), InlineProperty, HideLabel]
    public IActionAnimationCollection ActionAnimations = new();

    [BoxGroup("ANIMATIONS"), SerializeReference, ValidateInput(nameof(ValidateFallbackAnimation))]
    public IActionAnimation FallbackAnimation;

    [Required, SerializeReference, BoxGroup("INVENTORY")]
    public IInventory Inventory = new InlineInventory();

    /// <summary>
    /// How much experience points this unit gives on death
    /// </summary>
    [BoxGroup("DROP")]
    public int ExperiencePool;

    /// <summary>
    /// How many Credits this unit gives on death
    /// </summary>
    [BoxGroup("DROP")]
    public int CreditPool;

    /// <summary>
    /// Loot that the enemy drops
    /// </summary>
    [BoxGroup("DROP")]
    public List<Drop> DropItems;

    /// <summary>
    /// Stats after all mods, like level, equipment and status effects, have been applied
    /// </summary>
    public virtual Stats EffectiveStats => new Stats
    {
        HP = (int)(BaseStats.HP + BaseStats.HP * GetGrowthRateMultiplier(GrowthRate) * Level),
        MP = (int)(BaseStats.MP + BaseStats.MP * GetGrowthRateMultiplier(GrowthRate) * Level),
        Attack = (int)(BaseStats.Attack + BaseStats.Attack * GetGrowthRateMultiplier(GrowthRate) * Level),
        MagAttack = (int)(BaseStats.MagAttack + BaseStats.MagAttack * GetGrowthRateMultiplier(GrowthRate) * Level),
        Defense = (int)(BaseStats.Defense + BaseStats.Defense * GetGrowthRateMultiplier(GrowthRate) * Level),
        MagDefense = (int)(BaseStats.MagDefense + BaseStats.MagDefense * GetGrowthRateMultiplier(GrowthRate) * Level),
        Speed = BaseStats.Speed,
    };

    void UpdateDummyStats()
    {
        RegenHealthAndMana();
        _effectiveStatsPreview = EffectiveStats;
    }

    bool HasButton(GameObject go, ref string errorMessage)
    {
        return go != null &&  go.GetComponent<BattleCharacterController>();
    }

    bool ValidateActionAnimation(IActionAnimationCollection actionAnimation, ref string errorMessage)
    {
        return actionAnimation.Validate(this, ref errorMessage);
    }

    bool ValidateFallbackAnimation(IActionAnimation actionAnimation, ref string errorMessage)
    {
        if (actionAnimation == null)
        {
            errorMessage = "Value is null";
            return false;
        }

        return actionAnimation.Validate(null, this, ref errorMessage);
    }

    void UpdateExperienceFromLevel()
    {
        UpdateDummyStats();
        Experience = GetAmountOfXPForLevel(Level);
    }

    void UpdateLevelFromExperience()
    {
        UpdateDummyStats();
        Level = GetAmountOfLevelForXP(Experience);
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

    protected virtual void Awake()
    {
        RegenHealthAndMana();
    }

    protected void RegenHealthAndMana()
    {
        CurrentHP = EffectiveStats.HP;
        CurrentMP = EffectiveStats.MP;
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
        Status.Blind => IsBlinded,
        Status.Silenced => IsSilenced,
        Status.Furored => IsFurored,
        Status.Paralysed => IsParalysed,
        Status.PhysDown => IsPhysDown,
        Status.MagDown => IsMagDown,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    public int GetStatusResistance(Status status) => status switch
    {
        Status.Blind => ResistBLIND,
        Status.Silenced => ResistSILENCE,
        Status.Furored => ResistFUROR,
        Status.Paralysed => ResistPARALYSIS,
        Status.PhysDown => ResistPHYSICAL,
        Status.MagDown => ResistMAGICAL,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    [Serializable]
    public struct Drop
    {
        public BaseItem Item;
        [ValidateInput(nameof(ValidateCount), "Must be greater than 0!")]
        public uint Count;

        [ValidateInput(nameof(ValidateDropRate), "Must be greater than 0!"), Range(0, 100)]
        public int DropRatePercent;

        bool ValidateCount(uint count) => count > 0;
        bool ValidateDropRate(uint percent) => percent > 0;
    }
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