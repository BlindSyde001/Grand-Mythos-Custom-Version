using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEnemyController : BattleCharacterController
{
    // VARIABLES
    [SerializeField]
    internal EnemyExtension myEnemy;

    // UPDATES
    private void Update()
    {
        if (eventManager._GameState == GameState.BATTLE)
        {
            switch (BattleStateMachine._CombatState)
            {
                case CombatState.START:
                    break;

                case CombatState.ACTIVE:
                    break;

                case CombatState.WAIT:
                    anim.Play("Stance");
                    break;

                case CombatState.END:
                    break;
            }
        }
    }

    // METHODS
    public override void ActiveStateBehaviour()
    {
        myEnemy._ActionChargeAmount += myEnemy._ActionRechargeSpeed * Time.deltaTime;
        myEnemy._ActionChargeAmount = Mathf.Clamp(myEnemy._ActionChargeAmount, 0, 100);
    }
    public override void DieCheck()
    {
        if (myEnemy._CurrentHP <= 0)
        {
            myEnemy._CurrentHP = 0;
            myEnemy._ActionChargeAmount = 0;
            FindObjectOfType<BattleStateMachine>().CheckCharIsDead(this);
        }
    }
}
