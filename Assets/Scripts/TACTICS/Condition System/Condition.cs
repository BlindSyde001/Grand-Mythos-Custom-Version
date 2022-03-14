using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterActiveStatus { ACTIVE, DOWNED};
public class Condition : ScriptableObject
{
    public CharacterType targetType;
    public CharacterActiveStatus targetStatus;
    public int cndID;
    public virtual bool ConditionCheck(BattleCharacterController target)
    {
        return true;
    }
}
