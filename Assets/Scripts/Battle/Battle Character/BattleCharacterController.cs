using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public abstract class BattleCharacterController :MonoBehaviour
{
    // VARIABLES
    [SerializeField]
    internal Animator anim;

    // METHODS
    public virtual void ActiveStateBehaviour()
    {

    }
    public virtual void DieCheck()
    {

    }
}
