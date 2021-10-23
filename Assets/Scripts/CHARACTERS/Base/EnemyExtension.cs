using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyExtension : CharacterCircuit
{
    [SerializeField]
    internal int experiencePool; // How much EXP the enemy Gives

    protected internal void PerformEnemyAction(Action action, CharacterCircuit target)
    {
        foreach (ActionBehaviour aBehaviour in action._Behaviours)
        {
            aBehaviour.PreActionTargetting(this, action, target);
        }
        ConsumeActionCharge();
    }
    public override void AssignStats()
    {
        Debug.Log(this.name + " Activated");
        charName = _CSA._Name;
        characterType = _CSA._CharacterType;
        _MaxHP = _CSA._BaseHP;
        _MaxMP = _CSA._BaseMP;
        _Attack = _CSA._BaseAttack;
        _MagAttack = _CSA._BaseMagAttack;
        _Defense = _CSA._BaseDefense;
        _MagDefense = _CSA._BaseMagDefense;

        _ActionRechargeSpeed = 20;
        _CurrentHP = _MaxHP;
        _CurrentMP = _MaxMP;
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
