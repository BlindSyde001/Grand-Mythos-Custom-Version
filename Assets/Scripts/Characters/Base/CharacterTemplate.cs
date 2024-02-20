using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class CharacterTemplate : MonoBehaviour
{
    [SerializeField]
    [PropertyOrder(0)]
    public CharacterStatsAsset _CSA;

    [PropertyOrder(0)]
    public string charName;

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
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Left/CURRENT")]
    [PropertyOrder(1)]
    [GUIColor(0f, 1f, 0.3f)]
    public int _CurrentHP;

    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Left")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Left/CURRENT")]
    [PropertyOrder(1)]
    [GUIColor(0f, 0.8f, 1f)]
    public int _CurrentMP;

    [SerializeField]
    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Right")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Right/MAXIMUM")]
    [PropertyOrder(1)]
    [GUIColor(0f, 1f, 0.3f)]
    protected private int _MaxHP;
    public int MaxHP { get => _MaxHP; }

    [SerializeField]
    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Right")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Right/MAXIMUM")]
    [PropertyOrder(1)]
    [GUIColor(0f, 0.8f, 1f)]
    protected private int _MaxMP;
    public int MaxMP { get => _MaxMP; }

    [SerializeField]
    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Left")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Left/OFFENSIVE")]
    [PropertyOrder(1)]
    [GUIColor(1f, 0.2f, 0f)]
    protected private int _Attack;
    public int Attack { get => _Attack;  }
    [SerializeField]
    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Left")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Left/OFFENSIVE")]
    [PropertyOrder(1)]
    [GUIColor(1f, 0.2f, 0f)]
    protected private int _MagAttack;
    public int MagAttack { get => _MagAttack; }
    [SerializeField]
    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Right")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Right/DEFENSIVE")]
    [PropertyOrder(1)]
    [GUIColor(1f, 1f, 0f)]
    protected private int _Defense;
    public int Defense { get => _Defense; }
    [SerializeField]
    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Right")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Right/DEFENSIVE")]
    [PropertyOrder(1)]
    [GUIColor(1f, 1f, 0f)]
    protected private int _MagDefense;
    public int MagDefense { get => _MagDefense; }
    [SerializeField]
    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Right")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Right/DEFENSIVE")]
    [PropertyOrder(1)]
    [GUIColor(1f, 1f, 0f)]
    protected private int _Speed;
    public int Speed { get => _Speed; }

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

    [BoxGroup("ATB")]
    [PropertyOrder(4)]
    public float ActionRechargeSpeed;

    [BoxGroup("ATB")]
    [PropertyRange(0, 100)]
    [PropertyOrder(4)]
    public float ActionChargePercent;

    [BoxGroup("ATB")]
    [PropertyOrder(4)]
    public uint ActionChargeMax = 4;

    [NonSerialized]
    public EvaluationContext Context = new();

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
    /// How much EXP the enemy Gives
    /// </summary>
    public int experiencePool;
    /// <summary>
    /// How many Credits the enemy Gives
    /// </summary>
    public int creditPool;

    /// <summary>
    /// Loot that the enemy drops
    /// </summary>
    public List<ItemCapsule> DropItems;
    public List<float> DropRate;

    // UPDATES
    protected virtual void Awake()
    {
        AssignStats();
    }

    // METHODS
    public virtual void AssignStats()
    {
        charName = _CSA._Name;
        _MaxHP = _CSA._BaseHP;
        _MaxMP = _CSA._BaseMP;
        _Attack = _CSA._BaseAttack;
        _MagAttack = _CSA._BaseMagAttack;
        _Defense = _CSA._BaseDefense;
        _MagDefense = _CSA._BaseMagDefense;

        ActionRechargeSpeed = _CSA._BaseSpeed;
        _CurrentHP = _MaxHP;
        _CurrentMP = _MaxMP;
    }

    public bool IsHostileTo(CharacterTemplate character)
    {
        return Team.Allies.Contains(character.Team) == false;
    }

    public int GetAttribute(Attribute attribute) => attribute switch
    {
        Attribute.Health => _CurrentHP,
        Attribute.Mana => _CurrentMP,
        Attribute.HealthPercent => Mathf.CeilToInt((float)_CurrentHP / _MaxHP * 100f),
        Attribute.ManaPercent => Mathf.CeilToInt((float)_CurrentMP / _MaxMP * 100f),
        Attribute.MaxHealth => _MaxHP,
        Attribute.MaxMana => _MaxMP,
        Attribute.Attack => _Attack,
        Attribute.MagicAttack => _MagAttack,
        Attribute.Defense => _Defense,
        Attribute.MagicDefense => _MagDefense,
        Attribute.Speed => _Speed,
        _ => throw new ArgumentOutOfRangeException(nameof(attribute), attribute, null)
    };

    public void SetAttribute(Attribute attribute, int value)
    {
        switch(attribute)
        {
            case Attribute.Health: _CurrentHP = Mathf.Clamp(value, 0, _MaxHP); break;
            case Attribute.Mana: _CurrentMP = Mathf.Clamp(value, 0, _MaxMP); break;
            case Attribute.HealthPercent: _CurrentHP = Mathf.CeilToInt(value / 100f * _MaxHP); break;
            case Attribute.ManaPercent: _CurrentMP = Mathf.CeilToInt(value / 100f * _MaxMP); break;
            case Attribute.MaxHealth: _MaxHP = value; break;
            case Attribute.MaxMana: _MaxMP = value; break;
            case Attribute.Attack: _Attack = value; break;
            case Attribute.MagicAttack: _MagAttack = value; break;
            case Attribute.Defense: _Defense = value; break;
            case Attribute.MagicDefense: _MagDefense = value; break;
            case Attribute.Speed: _Speed = value; break;
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