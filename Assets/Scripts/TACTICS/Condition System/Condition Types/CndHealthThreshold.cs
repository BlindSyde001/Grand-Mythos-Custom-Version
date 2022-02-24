using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CndHealthThreshold : Condition
{
    private double percentage;
    public float healthThreshold;
    public bool _IsGreaterThan;
    public override bool ConditionCheck(BattleCharacterController target)
    {
        CharacterTemplate tempToUse;
        switch (target.myType)
        {
            case BattleCharacterController.ControllerType.HERO:
                {
                    BattleHeroController a = target as BattleHeroController;
                    tempToUse = a.myHero;
                    break;
                }

            default:
                {
                    BattleEnemyController a = target as BattleEnemyController;
                    tempToUse = a.myEnemy;
                    break;
                }
        }
        percentage = 1.0 * tempToUse._CurrentHP / tempToUse.MaxHP;
        if (_IsGreaterThan ? percentage >= healthThreshold / 100 :
                             percentage <= healthThreshold / 100)
        {
            return true;
        }
        else return false;
    }
}
