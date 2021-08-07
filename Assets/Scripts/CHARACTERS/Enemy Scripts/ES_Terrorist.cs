using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ES_Terrorist : EnemyExtension
{
    // Alternates between attacking and using Grenade. Has an emergency potion when below 50%HP
    int potionCount = 1;
    private void Update()
    {
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

    // Basic Attack
    private void BasicAttack()
    {
        //_AvailableActions[0];
    }
    // Potion
    private void Potion()
    {
        // _AvailableActions[1];
    }
    // Grenade
    private void Grenade()
    {
        // AvailableActions[2];
    }
}
