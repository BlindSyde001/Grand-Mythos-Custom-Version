using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HeroExtension : CharacterCircuit
{
    [SerializeField]
    private HeroGambitController myGambitController;

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
