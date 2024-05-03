using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

[AddComponentMenu(" GrandMythos/CharacterTemplate")]
public class CharacterTemplate : MonoBehaviour
{
    [ValidateInput(nameof(ValidateName))]
    public string Name;

    [Required]
    public Team Team;

    [HorizontalGroup("ASSETS"), ValidateInput(nameof(HasBattleCharacterController), "Must have a BattleHeroModelController"), SerializeField, PreviewField(100)]
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
    public StatGrowth StatGrowth;

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

    [BoxGroup("SKILLS")]
    public SerializableHashSet<Skill> Skills;

    [BoxGroup("SKILLS"), CanBeNull]
    public LevelUnlocks LevelUnlocks;

    [BoxGroup("TACTICS"), Required, ListDrawerSettings(ShowFoldout = false), TableList, ItemCanBeNull]
    public Tactics[] Tactics = Array.Empty<Tactics>();

    [BoxGroup("ANIMATIONS")]
    [ValidateInput(nameof(ValidateActionAnimation)), InlineProperty, HideLabel]
    public IActionAnimationCollection ActionAnimations = new();

    [BoxGroup("ANIMATIONS/Fallback Animation")]
    [InfoBox("When this unit performs an action that hasn't been added to the list above, the following fallback animation will run")]
    [SerializeReference, ValidateInput(nameof(ValidateFallbackAnimation)), InlineProperty, HideLabel]
    public IActionAnimation FallbackAnimation;

    [Required, SerializeReference, BoxGroup("INVENTORY"), InlineProperty, HideLabel]
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
    public virtual Stats EffectiveStats => StatGrowth.ApplyGrowth(BaseStats, Level);

    public int ExperienceToNextLevel => ExperienceThreshold - Experience; // How Much (Relative) you need
    public int ExperienceThreshold => GetAmountOfXPForLevel(Level); // How Much (Total) you need
    public int PrevExperienceThreshold => GetAmountOfXPForLevel(Level-1);

    void UpdateDummyStats()
    {
        RegenHealthAndMana();
        _effectiveStatsPreview = EffectiveStats;
    }

    bool HasBattleCharacterController(GameObject go, ref string errorMessage)
    {
        return go != null && go.GetComponent<BattleCharacterController>();
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

    bool ValidateName(string name, ref string errorMessage)
    {
        if (string.IsNullOrEmpty(name))
        {
            errorMessage = "Name is empty";
            return false;
        }

        return true;
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

    public void LevelUpCheck()
    {
        while (Experience >= ExperienceThreshold)
        {
            Level++;
            RegenHealthAndMana();
            if (LevelUnlocks != null)
            {
                foreach (var skill in LevelUnlocks.GetSkillsForLevel((uint)Level))
                    Skills.Add(skill);
            }
        }
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