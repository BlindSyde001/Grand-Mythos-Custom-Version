using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public abstract class CharacterTemplate : MonoBehaviour
{
    // VARIABLES
    #region CHARACTER TEMPLATE VARIABLES
    [SerializeField]
    [PropertyOrder(0)]
    internal CharacterStatsAsset _CSA;
    [SerializeField]
    [PropertyOrder(0)]
    internal CharacterType characterType;

    [PropertyOrder(0)]
    public string charName;

    [SerializeField]
    [PreviewField(100)]
    [PropertyOrder(0)]
    [HideLabel]
    internal Sprite charPortrait;

    [SerializeField]
    [PreviewField(100)]
    [PropertyOrder(0)]
    [HideLabel]
    internal GameObject _CharacterModel;

    internal GameObject _MyInstantiatedModel;

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

    [PropertyOrder(4)]
    public float _ActionRechargeSpeed;
    [PropertyRange(0, 100)]
    [PropertyOrder(4)]
    public float _ActionChargeAmount;

    [SerializeField]
    [PropertyOrder(5)]
    protected internal Action _BasicAttack;
    [SerializeField]
    [PropertyOrder(5)]
    protected internal List<Action> _AvailableActions;
    #endregion

    // UPDATES
    protected virtual void Awake()
    {
        AssignStats();
    }

    // METHODS
    public virtual void AssignStats()
    {

    }
}
