using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CndGroupSize : Condition
{
    public int _GroupSizeThreshold;
    public override bool ConditionCheck(CharacterCircuit target)
    {
        // Check how many are in a group
        switch(targetType)
        {
            case CharacterType.CHARACTER:
                if (GameManager._instance._PartyMembersActive.Count > _GroupSizeThreshold)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            case CharacterType.ENEMY:
                if (GameManager._instance._PartyMembersActive.Count > _GroupSizeThreshold)
                {
                    return true;
                }
                else
                {
                    return false;
                }
        }
        return false;
    }
}
