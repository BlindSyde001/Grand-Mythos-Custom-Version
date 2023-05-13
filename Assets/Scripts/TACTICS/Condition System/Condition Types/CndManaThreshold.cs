using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CndManaThreshold : Condition
{
    public int manaThreshold;
    public bool _IsGreaterThan;
    public override bool ConditionCheck(BattleCharacterController target)
    {
        CharacterTemplate tempToUse;
        switch (target.myType)
        {
            case BattleCharacterController.ControllerType.HERO:
                {
                    BattleHeroModelController a = target as BattleHeroModelController;
                    tempToUse = a.myHero;
                    break;
                }

            default:
                {
                    BattleEnemyModelController a = target as BattleEnemyModelController;
                    tempToUse = a.myEnemy;
                    break;
                }
        }
        if (_IsGreaterThan ? tempToUse._CurrentMP / tempToUse.MaxMP >= manaThreshold / 100 :
                              tempToUse._CurrentMP / tempToUse.MaxMP <= manaThreshold / 100)
        {
            return true;
        }
        else return false;
    }
}
