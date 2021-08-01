using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition : ScriptableObject
{
    public virtual bool ConditionCheck(CharacterCircuit target)
    {
        return true;
    }
}
