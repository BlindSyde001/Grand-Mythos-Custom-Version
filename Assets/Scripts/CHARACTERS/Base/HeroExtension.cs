using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class HeroExtension : CharacterTemplate
{
    // VARIABLES
    #region BASE STATS
    internal virtual int BaseHP { get => _CSA._BaseHP; }
    internal virtual int BaseMP { get => _CSA._BaseMP; }
    internal virtual int BaseAttack { get => _CSA._BaseAttack; }
    internal virtual int BaseMagAttack { get => _CSA._BaseMagAttack; }
    internal virtual int BaseDefense { get => _CSA._BaseDefense; }
    internal virtual int BaseMagDefense { get => _CSA._BaseMagDefense; }
    internal virtual int BaseSpeed { get => _CSA._BaseSpeed; }
    #endregion
    #region LEVEL STATS
    [SerializeField]
    [PropertyRange(1, 100)]
    [BoxGroup("LEVEL ATTRIBUTES")]
    [PropertyOrder(2)]
    protected internal int _Level;
    public int Level { get { return _Level; }
                       set { _Level = Mathf.Clamp(value, 1, 100); }
    }
    [BoxGroup("LEVEL ATTRIBUTES")]
    [PropertyOrder(2)]
    public int _TotalExperience;
    [SerializeField]
    [BoxGroup("LEVEL ATTRIBUTES")]
    [PropertyOrder(2)]
    protected internal int _ExperienceToNextLevel { get { return ExperienceThreshold - _TotalExperience; } }  // How Much (Relative) you need
    internal int ExperienceThreshold { get { return (int)(15 * Mathf.Pow(_Level, 2.3f) + (15 * _Level)); } }  // How Much (Total) you need
    internal int PrevExperienceThreshold { get { return (int)(15 * Mathf.Pow((_Level-1), 2.3f) + (15 * (_Level-1))); } }

    private protected float _GrowthRateHyper = 0.5f;
    private protected float _GrowthRateStrong = 0.3f;
    private protected float _GrowthRateAverage = 0.2f;
    private protected float _GrowthRateWeak = 0.1f;
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
    [PropertyOrder(-1)]
    internal BattleHeroController myBattleHeroController;

    [SerializeField]
    [PreviewField(100)]
    [PropertyOrder(0)]
    [HideLabel]
    internal Sprite charBanner;

    [SerializeField]
    [PropertyOrder(5)]
    internal protected List<Action> _AllUsableActions;
    [SerializeField]
    [PropertyOrder(6)]
    internal protected HeroTacticController myTacticController;

    // UPDATES
    protected override void Awake()
    {
        InitializeCharacter();
        EquipStats();
    }

    // METHODS
    #region Initialization
    protected void InitializeCharacter()
    {
        charName = _CSA._Name;
        characterType = _CSA._CharacterType;

        _TotalExperience = _CSA._BaseExperience;
        LevelUpCheck();

        myTacticController.myHeroCtrlr = myBattleHeroController;
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
