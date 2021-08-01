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
    [PreviewField(100)]
    [HideLabel]
    internal GameObject _CharacterModel;

    [SerializeField]
    protected private int _Level;
    public int _Experience;

    public int _CurrentHP;
    public int _CurrentMP;

    [SerializeField]
    protected private int _MaxHP;
    public int MaxHP { get => _MaxHP; }

    [SerializeField]
    protected private int _MaxMP;
    public int MaxMP { get => _MaxMP; }

    [SerializeField]
    protected private int _Attack;
    public int Attack { get => _Attack;  }
    [SerializeField]
    protected private int _MagAttack;
    public int MagAttack { get => _MagAttack; }
    [SerializeField]
    protected private int _Defense;
    public int Defense { get => _Defense; }
    [SerializeField]
    protected private int _MagDefense;
    public int MagDefense { get => _MagDefense; }

    public float _ActionRechargeSpeed = 20;
    public float _ActionChargeAmount;

    public List<Action> _AvailableActions;
    #endregion

    // UPDATES
    private void Awake()
    {
        AssignStats();
    }

    // METHODS
    public void ActiveStateBehaviour()
    {
        _ActionChargeAmount += _ActionRechargeSpeed * Time.deltaTime;
        _ActionChargeAmount = Mathf.Clamp(_ActionChargeAmount, 0, 100);
    }  // Charges Action Bar when in combat, allowing action when it is full
    public void AssignStats()
    {
        _Level = _CSA._BaseLevel;
        _MaxHP = _CSA._BaseHP * _Level;
        _MaxMP = _CSA._BaseMP * _Level;
        _Attack = _CSA._BaseAttack * _Level;
        _MagAttack = _CSA._BaseMagAttack * _Level;
        _Defense = _CSA._BaseDefense * _Level;
        _MagDefense = _CSA._BaseMagDefense * _Level;

        _CurrentHP = _MaxHP;
        _CurrentMP = _MaxMP;
    }
}
