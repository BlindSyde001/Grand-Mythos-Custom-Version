using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyExtension : CharacterTemplate
{

    [SerializeField]
    internal BattleEnemyController myBattleEnemyController;

    [SerializeField]
    internal int experiencePool; // How much EXP the enemy Gives
    [SerializeField]
    internal int creditPool;     // How many Credits the enemy Gives

    public override void AssignStats()
    {
        charName = _CSA._Name;
        characterType = _CSA._CharacterType;
        _MaxHP = _CSA._BaseHP;
        _MaxMP = _CSA._BaseMP;
        _Attack = _CSA._BaseAttack;
        _MagAttack = _CSA._BaseMagAttack;
        _Defense = _CSA._BaseDefense;
        _MagDefense = _CSA._BaseMagDefense;

        _ActionRechargeSpeed = _CSA._BaseSpeed;
        _CurrentHP = _MaxHP;
        _CurrentMP = _MaxMP;
    }
    public virtual void EnemyAct()
    {

    }
    private protected bool CheckForHeroTarget()
    {
        if (BattleStateMachine._HeroesActive.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
