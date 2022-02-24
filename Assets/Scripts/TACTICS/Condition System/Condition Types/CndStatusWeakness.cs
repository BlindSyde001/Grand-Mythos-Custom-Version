using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cnd")]
public class CndStatusWeakness : Condition
{
    public bool blindVuln;
    public bool silenceVuln;
    public bool furorVuln;
    public bool paralysisVuln;
    public bool physVuln;
    public bool magVuln;

    public bool isGreaterThan;

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

        if (blindVuln)
        {
            if (isGreaterThan ? tempToUse._ResistBLIND > 0 : tempToUse._ResistBLIND < 0)
            {
                return true;
            }
        }
        if (silenceVuln)
        {
            if (isGreaterThan ? tempToUse._ResistSILENCE > 0 : tempToUse._ResistSILENCE < 0)
            {
                return true;
            }
        }
        if (furorVuln)
        {
            if (isGreaterThan ? tempToUse._ResistFUROR > 0 : tempToUse._ResistFUROR < 0)
            {
                return true;
            }
        }
        if (paralysisVuln)
        {
            if (isGreaterThan ? tempToUse._ResistPARALYSIS > 0 : tempToUse._ResistPARALYSIS < 0)
            {
                return true;
            }
        }
        if (physVuln)
        {
            if (isGreaterThan ? tempToUse._ResistPHYSICAL > 0 : tempToUse._ResistPHYSICAL < 0)
            {
                return true;
            }
        }
        if (magVuln)
        {
            if (isGreaterThan ? tempToUse._ResistMAGICAL > 0 : tempToUse._ResistMAGICAL < 0)
            {
                return true;
            }
        }
        return false;
    }
}
