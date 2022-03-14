using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cndIsDowned : Condition
{
    public override bool ConditionCheck(BattleCharacterController target)
    {
        switch(targetType)
        {
            case CharacterType.CHARACTER:
                BattleHeroController hero = target as BattleHeroController;
                if (BattleStateMachine._HeroesDowned.Contains(hero))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            case CharacterType.ENEMY:
                BattleEnemyController enemy = target as BattleEnemyController;
                if (BattleStateMachine._EnemiesDowned.Contains(enemy))
                {
                    return true;
                }
                else
                    return false;
        }
        return false;
    }
}
