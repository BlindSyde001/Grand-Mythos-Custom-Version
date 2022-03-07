using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ES_Rewired_Security_Bot : EnemyExtension
{
    // Smacks hard. Is weak to Lightning based attacks.

    // VARIABLES

    // METHODS
    public override void EnemyAct()
    {
        if (_ActionChargeAmount == 100)
        {
            BasicAttack();
        }
    }
    private void BasicAttack()
    {
        //Debug.Log("Check: " + CheckForHeroTarget());
        if (CheckForHeroTarget())
        {
            //Debug.Log(charName + " has Attacked!");
            int x = Random.Range(0, BattleStateMachine._HeroesActive.Count);
            StartCoroutine(myBattleEnemyController.PerformEnemyAction(_BasicAttack, BattleStateMachine._HeroesActive[x]));
        }
    }
}
