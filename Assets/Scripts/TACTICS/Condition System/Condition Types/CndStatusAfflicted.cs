using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CndStatusAfflicted : Condition
{
    public bool isBlinded;
    public bool isSilenced;
    public bool isFurored;
    public bool isParalysed;
    public bool isPhysDown;
    public bool isMagDown;
    public override bool ConditionCheck(CharacterCircuit target)
    {
        if(isBlinded)
        {
            return target._IsBlinded;
        }
        if (isSilenced)
        {
            return target._IsSilenced;
        }
        if (isFurored)
        {
            return target._IsFurored;
        }
        if (isParalysed)
        {
            return target._IsParalysed;
        }
        if (isPhysDown)
        {
            return target._IsPhysDown;
        }
        if (isMagDown)
        {
            return target._IsMagDown;
        }
        else return false;
    }
}
