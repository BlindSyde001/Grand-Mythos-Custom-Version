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

    public override bool ConditionCheck(CharacterTemplate target)
    {
        if (blindVuln)
        {
            if (isGreaterThan ? target._ResistBLIND > 0 : target._ResistBLIND < 0)
            {
                return true;
            }
        }
        if (silenceVuln)
        {
            if (isGreaterThan ? target._ResistSILENCE > 0 : target._ResistSILENCE < 0)
            {
                return true;
            }
        }
        if (furorVuln)
        {
            if (isGreaterThan ? target._ResistFUROR > 0 : target._ResistFUROR < 0)
            {
                return true;
            }
        }
        if (paralysisVuln)
        {
            if (isGreaterThan ? target._ResistPARALYSIS > 0 : target._ResistPARALYSIS < 0)
            {
                return true;
            }
        }
        if (physVuln)
        {
            if (isGreaterThan ? target._ResistPHYSICAL > 0 : target._ResistPHYSICAL < 0)
            {
                return true;
            }
        }
        if (magVuln)
        {
            if (isGreaterThan ? target._ResistMAGICAL > 0 : target._ResistMAGICAL < 0)
            {
                return true;
            }
        }
        return false;
    }
}
