using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CndStatus : Condition
{
    public override bool ConditionCheck(CharacterCircuit target)
    {
        // Check for a status effect applied on the target
        return false;
    }
}
