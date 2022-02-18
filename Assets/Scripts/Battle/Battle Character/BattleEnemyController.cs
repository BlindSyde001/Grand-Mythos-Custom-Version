using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEnemyController : BattleCharacterController
{
    [SerializeField]
    private EnemyExtension myEnemy;


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
            FindObjectOfType<BattleStateMachine>().CheckCharIsDead(myEnemy);
        }
    }

}
