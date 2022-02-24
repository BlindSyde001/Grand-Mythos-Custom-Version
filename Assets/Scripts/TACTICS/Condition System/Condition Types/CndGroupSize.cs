using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CndGroupSize : Condition
{
    public int _GroupSizeThreshold;
    public override bool ConditionCheck(BattleCharacterController target)
    {
        // Check how many are in a group
        switch(targetType)
        {
            case CharacterType.CHARACTER:
                if (BattleStateMachine._HeroesActive.Count > _GroupSizeThreshold)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            case CharacterType.ENEMY:
                if (BattleStateMachine._HeroesActive.Count > _GroupSizeThreshold)
                {
                    return true;
                }
                else
                {
                    return false;
                }
        }
        return false;
    }
}
