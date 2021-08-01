using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CndHealthThreshold : Condition
{
    public int healthThreshold;
    public bool greaterThanCheck;
    public override bool ConditionCheck(CharacterCircuit target)
    {
        if (greaterThanCheck == true)
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
