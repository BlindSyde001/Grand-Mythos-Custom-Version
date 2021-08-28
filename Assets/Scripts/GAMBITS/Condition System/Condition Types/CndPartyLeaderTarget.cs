using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CndPartyLeaderTarget : Condition
{
    public override bool ConditionCheck(CharacterCircuit target)
    {
        // Check who is the target of Player Controller's target
        return false;
    }
}
