using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ES_Terrorist_Gunner : EnemyExtension
{
    // Alternates between attacking and using Grenade. Has an emergency potion when below 50% HP

    // VARIABLES
    int potionCount = 1;

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
                if(x == 0)
                {
                    BasicAttack();
                }
                else if(x == 1)
                {
                    Grenade();
                }
            }
        }   

    }
    private void BasicAttack()
    {
        if(CheckForHeroTarget())
         {
            Debug.Log(this.name + " Has Attacked!");
            int x = Random.Range(0, BattleStateMachine._HeroesActive.Count);
            PerformEnemyAction(_BasicAttack, BattleStateMachine._HeroesActive[x]);
        }
    }
    private void Potion()
    {
        Debug.Log(this.name + " Has used a Potion");
        PerformEnemyAction(_AvailableActions[0], this);
    }
    private void Grenade()
    {
        if(CheckForHeroTarget())
        {
            Debug.Log(this.name + " Has used a Grenade!");
            int x = Random.Range(0, BattleStateMachine._HeroesActive.Count);
            PerformEnemyAction(_AvailableActions[1], BattleStateMachine._HeroesActive[x]);
        }
    }
}
