using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCircuit : MonoBehaviour
{
    // VARIABLES
    private EventManager EM;

    [SerializeField]
    internal CharacterStatsAsset _CSA;
    [SerializeField]
    internal GameObject _CharacterModel;

    public int _CurrentHP;
    public int _MaxHP;
    public int _MaxMP;
    public int _Attack;
    public int _Defense;

    [SerializeField]
    internal float _ActionRechargeSpeed;
    public float _ActionChargeAmount;

    // UPDATES
    private void Awake()
    {
        EM = FindObjectOfType<EventManager>();
    }
    private void Start()
    {
        AssignStats();
    }
    private void Update()
    {
        if(EM._BattleState == BattleState.ACTIVE)
        ActiveBattle();
    }

    // METHODS
    private void AssignStats()
    {
        _MaxHP = _CSA._Health;
        _MaxMP = _CSA._Mana;
        _Attack = _CSA._Attack;
        _Defense = _CSA._Defense;
    }   // Assigns the character's stats from the Data Asset
    private void ActiveBattle()
    {
        _ActionChargeAmount += _ActionRechargeSpeed * Time.deltaTime;
        _ActionChargeAmount = Mathf.Clamp(_ActionChargeAmount, 0, 100);
        if (_ActionChargeAmount >= 100)
        {
            print("ACTION READY");
        }
    }  // Charges Action Bar when in combat, allowing action when it is full
}
