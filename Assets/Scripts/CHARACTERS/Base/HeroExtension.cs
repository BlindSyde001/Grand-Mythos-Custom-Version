using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class HeroExtension : CharacterTemplate
{
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
    [SerializeField]
    [BoxGroup("LEVEL ATTRIBUTES")]
    [PropertyOrder(2)]
    public int ExperienceToNextLevel => ExperienceThreshold - Experience; // How Much (Relative) you need
    public int ExperienceThreshold => GetAmountOfXPForLevel(_Level); // How Much (Total) you need
    public int PrevExperienceThreshold => GetAmountOfXPForLevel(_Level-1);
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

    public override Stats EffectiveStats
    {
        get
        {
            var stats = base.EffectiveStats;
            stats.HP += equipHP;
            stats.MP += equipMP;
            stats.Attack += equipAttack;
            stats.MagAttack += equipMagAttack;
            stats.Defense += equipDefense;
            stats.MagDefense += equipMagDefense;
            stats.Speed += equipSpeed;
            return stats;
        }
    }

    // UPDATES
    protected override void Awake()
    {
        InitializeCharacter();
        EquipStats();
        base.Awake();
    }

    // METHODS
    #region Initialization
    protected void InitializeCharacter()
    {
        LevelUpCheck();
    }
    public void InitializeLevel()
    {
        _Level = StartingLevel;
        if (Experience >= ExperienceThreshold)
        {
            _Level++;
            LevelUpCheck();
            RegenHealthAndMana();
        }
    }
    #endregion
    #region Stats & Levelling Up
    public void LevelUpCheck()
    {
        if (Experience >= ExperienceThreshold)
        {
            _Level++;
            LevelUpCheck();
            RegenHealthAndMana();
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