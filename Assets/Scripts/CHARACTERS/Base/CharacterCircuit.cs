using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class CharacterCircuit : MonoBehaviour
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

    [BoxGroup("ELEMENTAL ATTRIBUTES")]
    [PropertyRange(-100, 100)]
    [PropertyOrder(1.1f)]
    public int _AffinityFIRE;

    [BoxGroup("ELEMENTAL ATTRIBUTES")]
    [PropertyRange(-100, 100)]
    [PropertyOrder(1.1f)]
    public int _AffinityICE;

    [BoxGroup("ELEMENTAL ATTRIBUTES")]
    [PropertyRange(-100, 100)]
    [PropertyOrder(1.1f)]
    public int _AffinityLIGHTNING;

    [BoxGroup("ELEMENTAL ATTRIBUTES")]
    [PropertyRange(-100, 100)]
    [PropertyOrder(1.1f)]
    public int _AffinityWATER;

    [BoxGroup("STATUS ATTRIBUTES")]
    [PropertyRange(-100, 100)]
    [PropertyOrder(1.2f)]
    public int _ResistBLIND;

    [BoxGroup("STATUS ATTRIBUTES")]
    [PropertyRange(-100, 100)]
    [PropertyOrder(1.2f)]
    public int _ResistSILENCE;

    [BoxGroup("STATUS ATTRIBUTES")]
    [PropertyRange(-100, 100)]
    [PropertyOrder(1.2f)]
    public int _ResistFUROR;

    [BoxGroup("STATUS ATTRIBUTES")]
    [PropertyRange(-100, 100)]
    [PropertyOrder(1.2f)]
    public int _ResistPHYSICAL;

    [BoxGroup("STATUS ATTRIBUTES")]
    [PropertyRange(-100, 100)]
    [PropertyOrder(1.2f)]
    public int _ResistMAGICAL;

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
    public void ConsumeActionCharge()
    {
        _ActionChargeAmount = 0;
    }
    public virtual void ActiveStateBehaviour()
    {
        _ActionChargeAmount += _ActionRechargeSpeed * Time.deltaTime;
        _ActionChargeAmount = Mathf.Clamp(_ActionChargeAmount, 0, 100);
    }  // Charges Action Bar when in combat, allowing action when it is full
    public virtual void AssignStats()
    {

    }
    public virtual void DieCheck()
    {

    }
}
