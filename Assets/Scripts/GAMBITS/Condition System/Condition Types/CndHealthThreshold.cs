using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CndHealthThreshold : Condition
{
    public int healthThreshold;
    public bool _IsGreaterThan;
    public override bool ConditionCheck(CharacterCircuit target)
    {
        if (_IsGreaterThan == true)
        {
            if (target._CurrentHP / target.MaxHP >= healthThreshold / 100)
            {
                return true;
            }
            else return false;
        }
        else
        {
            if (target._CurrentHP / target.MaxHP <= healthThreshold / 100)
            {
                return true;
            }
            else return false;
        }
    }
}
