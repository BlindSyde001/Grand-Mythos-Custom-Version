using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class HeroExtension : CharacterTemplate
{
    // VARIABLES
    #region BASE STATS
    public int BaseHP => (int)(_CSA._BaseHP * (1 + GetGrowthRateMultiplier(GrowthRate)) * _Level);
    public int BaseMP => (int)(_CSA._BaseMP * (1 + GetGrowthRateMultiplier(GrowthRate)) * _Level);
    public int BaseAttack => (int)(_CSA._BaseAttack * (1 + GetGrowthRateMultiplier(GrowthRate)) * _Level);
    public int BaseMagAttack => (int)(_CSA._BaseMagAttack * (1 + GetGrowthRateMultiplier(GrowthRate)) * _Level);
    public int BaseDefense => (int)(_CSA._BaseDefense * (1 + GetGrowthRateMultiplier(GrowthRate)) * _Level);
    public int BaseMagDefense => (int)(_CSA._BaseMagDefense * (1 + GetGrowthRateMultiplier(GrowthRate)) * _Level);
    public int BaseSpeed => _CSA._BaseSpeed;
    #endregion

    #region LEVEL STATS
    [SerializeField]
    [PropertyRange(1, 100)]
    [BoxGroup("LEVEL ATTRIBUTES")]
    [PropertyOrder(2)]
    protected internal int _Level;
    public int Level
    {
        get => _Level;
        set => _Level = Mathf.Clamp(value, 1, 100);
    }
    [BoxGroup("LEVEL ATTRIBUTES")]
    [PropertyOrder(2)]
    public int _TotalExperience;
    [SerializeField]
    [BoxGroup("LEVEL ATTRIBUTES")]
    [PropertyOrder(2)]
    public int ExperienceToNextLevel => ExperienceThreshold - _TotalExperience; // How Much (Relative) you need
    public int ExperienceThreshold => (int)(15 * Mathf.Pow(_Level, 2.3f) + (15 * _Level)); // How Much (Total) you need
    public int PrevExperienceThreshold => (int)(15 * Mathf.Pow((_Level-1), 2.3f) + (15 * (_Level-1)));
    #endregion
    #region EQUIPMENT STATS
    [TitleGroup("EQUIPMENT ATTRIBUTES")]
    [HorizontalGroup("EQUIPMENT ATTRIBUTES/Split")]
    [SerializeField]
    [VerticalGroup("EQUIPMENT ATTRIBUTES/Split/Left")]
    [BoxGroup("EQUIPMENT ATTRIBUTES/Split/Left/Equipment")]
    [LabelWidth(100)]
    [PropertyOrder(3)]
    protected internal Weapon _Weapon;

    [TitleGroup("EQUIPMENT ATTRIBUTES")]
    [SerializeField]
    internal Weapon.WeaponType myWeaponType;

    [SerializeField]
    [VerticalGroup("EQUIPMENT ATTRIBUTES/Split/Left")]
    [BoxGroup("EQUIPMENT ATTRIBUTES/Split/Left/Equipment")]
    [LabelWidth(100)]
    [PropertyOrder(3)]
    protected internal Armour _Armour;

    [TitleGroup("EQUIPMENT ATTRIBUTES")]
    [SerializeField]
    internal Armour.ArmourType myArmourType;

    [SerializeField]
    [VerticalGroup("EQUIPMENT ATTRIBUTES/Split/Left")]
    [BoxGroup("EQUIPMENT ATTRIBUTES/Split/Left/Equipment")]
    [LabelWidth(100)]
    [PropertyOrder(3)]
    protected internal Accessory _AccessoryOne;

    [SerializeField]
    [VerticalGroup("EQUIPMENT ATTRIBUTES/Split/Left")]
    [BoxGroup("EQUIPMENT ATTRIBUTES/Split/Left/Equipment")]
    [LabelWidth(100)]
    [PropertyOrder(3)]
    protected internal Accessory _AccessoryTwo;

    [SerializeField]
    [VerticalGroup("EQUIPMENT ATTRIBUTES/Split/Right")]
    [BoxGroup("EQUIPMENT ATTRIBUTES/Split/Right/Total Stats")]
    [LabelWidth(120)]
    [PropertyOrder(3)]
    protected private int equipHP;
    [SerializeField]
    [VerticalGroup("EQUIPMENT ATTRIBUTES/Split/Right")]
    [BoxGroup("EQUIPMENT ATTRIBUTES/Split/Right/Total Stats")]
    [LabelWidth(120)]
    [PropertyOrder(3)]
    protected private int equipMP;
    [SerializeField]
    [VerticalGroup("EQUIPMENT ATTRIBUTES/Split/Right")]
    [BoxGroup("EQUIPMENT ATTRIBUTES/Split/Right/Total Stats")]
    [LabelWidth(120)]
    [PropertyOrder(3)]
    protected private int equipAttack;

    [SerializeField]
    [VerticalGroup("EQUIPMENT ATTRIBUTES/Split/Right")]
    [BoxGroup("EQUIPMENT ATTRIBUTES/Split/Right/Total Stats")]
    [LabelWidth(120)]
    [PropertyOrder(3)]
    protected private int equipMagAttack;

    [SerializeField]
    [VerticalGroup("EQUIPMENT ATTRIBUTES/Split/Right")]
    [BoxGroup("EQUIPMENT ATTRIBUTES/Split/Right/Total Stats")]
    [LabelWidth(120)]
    [PropertyOrder(3)]
    protected private int equipDefense;

    [SerializeField]
    [VerticalGroup("EQUIPMENT ATTRIBUTES/Split/Right")]
    [BoxGroup("EQUIPMENT ATTRIBUTES/Split/Right/Total Stats")]
    [LabelWidth(120)]
    [PropertyOrder(3)]
    protected private int equipMagDefense;

    [SerializeField]
    [VerticalGroup("EQUIPMENT ATTRIBUTES/Split/Right")]
    [BoxGroup("EQUIPMENT ATTRIBUTES/Split/Right/Total Stats")]
    [LabelWidth(120)]
    [PropertyOrder(3)]
    protected private int equipSpeed;

    internal int EquipHP { get => equipHP; }
    internal int EquipMP { get => equipMP; }
    internal int EquipAttack { get => equipAttack; }
    internal int EquipMagAttack { get => equipMagAttack; }
    internal int EquipDefense { get => equipDefense; }
    internal int EquipMagDefense { get => equipMagDefense; }
    internal int EquipSpeed { get => equipSpeed; }
    #endregion

    [SerializeField]
    [PreviewField(100)]
    [PropertyOrder(0)]
    [HideLabel]
    internal Sprite charBanner;

    public GrowthRate GrowthRate;

    static float GetGrowthRateMultiplier(GrowthRate growthRate) => growthRate switch
    {
        GrowthRate.Average => 0.2f,
        GrowthRate.Strong => 0.3f,
        GrowthRate.Hyper => 0.5f,
        GrowthRate.Weak => 0.1f,
        GrowthRate.Fixed => 0f,
        _ => throw new ArgumentOutOfRangeException(nameof(growthRate), growthRate, null)
    };

    // UPDATES
    protected override void Awake()
    {
        InitializeCharacter();
        EquipStats();
        base.Awake();
    }
    public override void AssignStats()
    {
        _MaxHP = BaseHP + equipHP;
        _MaxMP = BaseMP + equipMP;
        _Attack = BaseAttack + equipAttack;
        _MagAttack = BaseMagAttack + equipMagAttack;
        _Defense = BaseDefense + equipDefense;
        _MagDefense = BaseMagDefense + equipMagDefense;
        _Speed = BaseSpeed + equipSpeed;

        ActionRechargeSpeed = Speed;
        _CurrentHP = _MaxHP;
        _CurrentMP = _MaxMP;
    }

    // METHODS
    #region Initialization
    protected void InitializeCharacter()
    {
        charName = _CSA._Name;

        _TotalExperience = _CSA._BaseExperience;
        LevelUpCheck();

        //myTacticController.myHeroCtrlr = myBattleHeroController;
    }
    public void InitializeLevel()
    {
        _Level = _CSA.startingLevel;
        if (_TotalExperience >= ExperienceThreshold)
        {
            _Level++;
            LevelUpCheck();
            AssignStats();
        }
    }
    #endregion
    #region Stats & Levelling Up
    public void LevelUpCheck()
    {
        if (_TotalExperience >= ExperienceThreshold)
        {
            _Level++;
            LevelUpCheck();
            AssignStats();
            if (SkillTree != null)
            {
                foreach (var skill in SkillTree.GetSkillsForLevel((uint)_Level))
                    Skills.Add(skill);
            }
        }
    }
    internal void EquipStats()
    {
        #region Reset Equip Stats
        equipAttack = 0;
        equipMagAttack = 0;
        equipDefense = 0;
        equipMagDefense = 0;

        equipHP = 0;
        equipMP = 0;
        #endregion
        #region Add Equipped Items to a Temporary List
        List<Equipment> tempEquip = new List<Equipment>();
        tempEquip.Add(_Weapon);
        if (_Armour != null)
            tempEquip.Add(_Armour);
        if (_AccessoryOne != null)
            tempEquip.Add(_AccessoryOne);
        if (_AccessoryTwo != null)
            tempEquip.Add(_AccessoryTwo);
        #endregion
        foreach(Equipment gear in tempEquip)
        {
            equipAttack += gear._EquipAttack;
            equipMagAttack += gear._EquipMagAttack;
            equipDefense += gear._EquipDefense;
            equipMagDefense += gear._EquipMagDefense;

            equipHP += gear._EquipHP;
            equipMP += gear._EquipMP;
        }
    }
    #endregion
}


public enum GrowthRate
{
    Average,
    Strong,
    Hyper,
    Weak,
    Fixed,
}