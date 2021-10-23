using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Blank condition")]
public class CndAny : Condition
{
    public override bool ConditionCheck(CharacterCircuit target)
    {
        return true;
    }
}
