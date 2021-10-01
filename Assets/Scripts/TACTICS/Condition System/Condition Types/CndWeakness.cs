using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CndWeakness : Condition
{
    public override bool ConditionCheck(CharacterCircuit target)
    {
        // Check for if the target has a Weakness
        return false;
    }
}
