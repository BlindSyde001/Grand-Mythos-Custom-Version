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
                if (BattleStateMachine._HeroesDowned.Count > 0)
                {
                    return true;
                }
                else
                    return false;

            case CharacterType.ENEMY:
                if (BattleStateMachine._EnemiesDowned.Count > 0)
                {
                    return true;
                }
                else
                    return false;
        }
        return false;
    }
}
