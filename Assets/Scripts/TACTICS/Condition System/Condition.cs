using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition : ScriptableObject
{
    public CharacterType targetType;
    public int cndID;
    public virtual bool ConditionCheck(CharacterCircuit target)
    {
        return true;
    }
}
