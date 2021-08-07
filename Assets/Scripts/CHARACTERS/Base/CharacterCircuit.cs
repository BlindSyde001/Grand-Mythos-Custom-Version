using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class CharacterCircuit : MonoBehaviour
{
    // VARIABLES
    #region CHARACTER TEMPLATE VARIABLES
    [SerializeField]
    protected internal CharacterStatsAsset _CSA;
    [SerializeField]
    internal CharacterType characterType;

    public string charName;

    [SerializeField]
    [PreviewField(100)]
    [HideLabel]
    internal GameObject _CharacterModel;

    [SerializeField]
    [PropertyRange(1, 100)]
    protected private int _Level;
    public int _Experience;

    [TitleGroup("CHARACTER ATTRIBUTES")]
    [HorizontalGroup("CHARACTER ATTRIBUTES/Split")]
    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Left")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Left/CURRENT")]
    [GUIColor(0f, 1f, 0.3f)]
    public int _CurrentHP;

    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Left")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Left/CURRENT")]
    [GUIColor(0f, 0.8f, 1f)]
    public int _CurrentMP;

    [SerializeField]
    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Right")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Right/MAXIMUM")]
    [GUIColor(0f, 1f, 0.3f)]
    protected private int _MaxHP;
    public int MaxHP { get => _MaxHP; }

    [SerializeField]
    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Right")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Right/MAXIMUM")]
    [GUIColor(0f, 0.8f, 1f)]
    protected private int _MaxMP;
    public int MaxMP { get => _MaxMP; }

    [SerializeField]
    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Left")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Left/OFFENSIVE")]
    [GUIColor(1f, 0.2f, 0f)]
    protected private int _Attack;
    public int Attack { get => _Attack;  }
    [SerializeField]
    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Left")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Left/OFFENSIVE")]
    [GUIColor(1f, 0.2f, 0f)]
    protected private int _MagAttack;
    public int MagAttack { get => _MagAttack; }
    [SerializeField]
    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Right")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Right/DEFENSIVE")]
    [GUIColor(1f, 1f, 0f)]
    protected private int _Defense;
    public int Defense { get => _Defense; }
    [SerializeField]
    [VerticalGroup("CHARACTER ATTRIBUTES/Split/Right")]
    [BoxGroup("CHARACTER ATTRIBUTES/Split/Right/DEFENSIVE")]
    [GUIColor(1f, 1f, 0f)]
    protected private int _MagDefense;
    public int MagDefense { get => _MagDefense; }

    public float _ActionRechargeSpeed;
    [PropertyRange(0, 100)]
    public float _ActionChargeAmount;

    [SerializeField]
    private protected List<Action> _AvailableActions;
    #endregion

    // UPDATES
    private void Awake()
    {
        AssignStats();
    }

    // METHODS
    public void ConsumeActionCharge()
    {
        _ActionChargeAmount = 0;
    }
    public void ActiveStateBehaviour()
    {
        _ActionChargeAmount += _ActionRechargeSpeed * Time.deltaTime;
        _ActionChargeAmount = Mathf.Clamp(_ActionChargeAmount, 0, 100);
    }  // Charges Action Bar when in combat, allowing action when it is full
    public void AssignStats()
    {
        _Level = _CSA._BaseLevel;
        charName = _CSA._Name;
        characterType = _CSA._CharacterType;
        _MaxHP = _CSA._BaseHP * _Level;
        _MaxMP = _CSA._BaseMP * _Level;
        _Attack = _CSA._BaseAttack * _Level;
        _MagAttack = _CSA._BaseMagAttack * _Level;
        _Defense = _CSA._BaseDefense * _Level;
        _MagDefense = _CSA._BaseMagDefense * _Level;

        _ActionRechargeSpeed = 20;
        _CurrentHP = _MaxHP;
        _CurrentMP = _MaxMP;
    }
    public virtual void DieCheck()
    {

    }
}
