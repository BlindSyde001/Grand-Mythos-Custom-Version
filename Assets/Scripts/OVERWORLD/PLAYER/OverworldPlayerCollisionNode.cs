using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OverworldPlayerCollisionNode : MonoBehaviour
{
    // VARIABLES
    [SerializeField]
    private OverworldPlayerCircuit _PlayerCircuit;
    [SerializeField]
    private PlayerControls playerControls;

    // UPDATES
    private void OnTriggerEnter(Collider other)
    {
        
    }
    private void OnTriggerExit(Collider other)
    {

    }
}
