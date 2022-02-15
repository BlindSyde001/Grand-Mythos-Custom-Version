using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ES_Terrorist_Gunner : EnemyExtension
{
    // throws a Grenade for big Party-wide Damage. Has an emergency Potion when below 50% HP

    // VARIABLES
    int potionCount = 1;
    int grenadeCount = 1;

    // METHODS

    public override void ActiveStateBehaviour()
    {
        base.ActiveStateBehaviour();
        if(_ActionChargeAmount == 100)
        {
            if(_CurrentHP <= MaxHP/2 && potionCount > 0)
            {
                potionCount--;
                Potion();
            }
            else
            {
               int x = Random.Range(0, 2);
                if(x == 0 && grenadeCount > 0)
                {
                    grenadeCount--;
                    Grenade();
                }
                else if(x == 1)
                {
                    BasicAttack();
                }
            }
        }   

    }
    private void BasicAttack()
    {
        if(CheckForHeroTarget())
         {
            int x = Random.Range(0, GameManager._instance._PartyMembersActive.Count);
            PerformEnemyAction(_BasicAttack, GameManager._instance._PartyMembersActive[x]);
        }
    }
    private void Potion()
    {
        PerformEnemyAction(_AvailableActions[0], this);
    }
    private void Grenade()
    {
        if(CheckForHeroTarget())
        {
            int x = Random.Range(0, GameManager._instance._PartyMembersActive.Count);
            PerformEnemyAction(_AvailableActions[1], GameManager._instance._PartyMembersActive[x]);
        }
    }
}
