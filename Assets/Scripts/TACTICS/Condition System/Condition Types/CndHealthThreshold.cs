using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CndHealthThreshold : Condition
{
    private double percentage;
    public float healthThreshold;
    public bool _IsGreaterThan;
    public override bool ConditionCheck(CharacterTemplate target)
    {
        percentage = 1.0 * target._CurrentHP / target.MaxHP;
        if (_IsGreaterThan ? percentage >= healthThreshold / 100 :
                             percentage <= healthThreshold / 100)
        {
            return true;
        }
        else return false;
    }
}
