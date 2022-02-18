using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OverworldPlayerCollisionNode : MonoBehaviour
{
    // VARIABLES
    [SerializeField]
    private OverworldPlayerCircuit _PlayerCircuit;

    // UPDATES
    private void OnTriggerEnter(Collider other)
    {
        
    }
    private void OnTriggerExit(Collider other)
    {

    }

    private void OnEnable()
    {
        if (InputManager._instance.playerInput != null)
            InputManager._instance.playerInput.actions.FindAction("Interact").performed += InteractionPressed;
    }
    private void OnDisable()
    {
        if(InputManager._instance.playerInput != null)
           InputManager._instance.playerInput.actions.FindAction("Interact").performed -= InteractionPressed;
    }

    // METHODS
    private void InteractionPressed(InputAction.CallbackContext context)
    {
        // Tell PlayerCircuit to Interact
        Debug.Log("Has Pressed");
    }
}
