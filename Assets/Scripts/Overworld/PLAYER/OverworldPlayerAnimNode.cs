using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class OverworldPlayerAnimNode : MonoBehaviour
{
    // VARIABLES
    [SerializeField]
    private OverworldPlayerCircuit _PlayerCircuit;

    [SerializeField]
    private Animator anim;

    // UPDATES
    private void FixedUpdate()
    {
        if(_PlayerCircuit.isMoving)
        {
            anim.Play("Run");
        }
        else
        {
            anim.Play("Idle");
        }
    }
}
