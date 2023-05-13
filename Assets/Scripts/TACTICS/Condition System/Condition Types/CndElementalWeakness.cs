using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CndElementalWeakness : Condition
{
    public bool fireVuln;
    public bool iceVuln;
    public bool waterVuln;
    public bool lightningVuln;

    public bool isGreaterThan;

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

        if (fireVuln)
        {
            if(isGreaterThan? tempToUse._AffinityFIRE > 0 : tempToUse._AffinityFIRE < 0)
            {
                return true;
            }
        }
        if(iceVuln)
        {
            if(isGreaterThan ? tempToUse._AffinityICE > 0 : tempToUse._AffinityICE < 0)
            {
                return true;
            }
        }
        if(waterVuln)
        {
            if(isGreaterThan ? tempToUse._AffinityWATER > 0 : tempToUse._AffinityWATER < 0)
            {
                return true;
            }
        }
        if(lightningVuln)
        {
            if(isGreaterThan ? tempToUse._AffinityLIGHTNING> 0 : tempToUse._AffinityLIGHTNING < 0)
            {
                return true;
            }
        }
        return false;
    }
}
