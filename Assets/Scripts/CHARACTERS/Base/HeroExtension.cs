using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class HeroExtension : CharacterCircuit
{
    // VARIABLES
    #region LEVEL STATS
    [SerializeField]
    [PropertyRange(1, 100)]
    [BoxGroup("LEVEL ATTRIBUTES")]
    [PropertyOrder(2)]
    protected internal int _Level;
    [BoxGroup("LEVEL ATTRIBUTES")]
    [PropertyOrder(2)]
    public int _TotalExperience;
    [SerializeField]
    [BoxGroup("LEVEL ATTRIBUTES")]
    [PropertyOrder(2)]
    protected int _ExperienceToNextLevel;
    protected int _ExperienceThreshold { get { return (int)(15 * Mathf.Pow(_Level, 2.3f) + (15 * _Level)); } }

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
    [SerializeField]
    [VerticalGroup("EQUIPMENT ATTRIBUTES/Split/Left")]
    [BoxGroup("EQUIPMENT ATTRIBUTES/Split/Left/Equipment")]
    [LabelWidth(100)]
    [PropertyOrder(3)]
    protected internal Armour _Armour;
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
    #endregion

    [SerializeField]
    [PropertyOrder(5)]
    private protected List<Action> _AllUsableActions;
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
    protected void InitializeCharacter()
    {
        charName = _CSA._Name;
        characterType = _CSA._CharacterType;

        _TotalExperience = _CSA._BaseExperience;
        LevelUpCheck();

        myTacticController.myHero = this;
    }
    protected void EquipStats()
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
    public override void ActiveStateBehaviour()
    {
        base.ActiveStateBehaviour();
        myTacticController.SetNextAction();
    }
    public void LevelUpCheck()
    {
        if (_TotalExperience >= _ExperienceThreshold)
        {
            _Level++;
            LevelUpCheck();
        }
        AssignStats();
        _ExperienceToNextLevel = _ExperienceThreshold - _TotalExperience;
    }
    public override void DieCheck()
    {
        if(_CurrentHP <= 0)
        {
            _CurrentHP = 0;
            _ActionChargeAmount = 0;
            FindObjectOfType<BattleStateMachine>().CheckCharIsDead(this);
        }
    }
}
