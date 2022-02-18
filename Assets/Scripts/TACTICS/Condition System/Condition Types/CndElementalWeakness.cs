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

    public override bool ConditionCheck(CharacterTemplate target)
    {
        if(fireVuln)
        {
            if(isGreaterThan? target._AffinityFIRE > 0 : target._AffinityFIRE < 0)
            {
                return true;
            }
        }
        if(iceVuln)
        {
            if(isGreaterThan ? target._AffinityICE > 0 : target._AffinityICE < 0)
            {
                return true;
            }
        }
        if(waterVuln)
        {
            if(isGreaterThan ? target._AffinityWATER > 0 : target._AffinityWATER < 0)
            {
                return true;
            }
        }
        if(lightningVuln)
        {
            if(isGreaterThan ? target._AffinityLIGHTNING> 0 : target._AffinityLIGHTNING < 0)
            {
                return true;
            }
        }
        return false;
    }
}
