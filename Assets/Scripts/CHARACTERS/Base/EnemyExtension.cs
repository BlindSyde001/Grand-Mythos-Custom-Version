using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyExtension : CharacterCircuit
{
    [SerializeField]
    internal int experiencePool; // How much EXP the enemy Gives

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
