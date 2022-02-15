using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CndManaThreshold : Condition
{
    public int manaThreshold;
    public bool _IsGreaterThan;
    public override bool ConditionCheck(CharacterCircuit target)
    {
        if (_IsGreaterThan ?  target._CurrentMP / target.MaxMP >= manaThreshold / 100 :
                              target._CurrentMP / target.MaxMP <= manaThreshold / 100)
        {
            return true;
        }
        else return false;
    }
}
