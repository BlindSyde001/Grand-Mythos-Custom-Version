using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OverworldPlayerInputNode : MonoBehaviour
{
    // VARIABLES
    [SerializeField]
    private OverworldPlayerCircuit _PlayerCircuit;

    // UPDATES
    private void Update()
    {
        switch(EventManager._instance._GameState)
        {
            case GameState.OVERWORLD:

                if (InputManager._instance.playerInput.actions.FindAction("Move").IsPressed())
                {
                    _PlayerCircuit.isMoving = true;
                    _PlayerCircuit.inputMovement = InputManager._instance.playerInput.actions.FindAction("Move").ReadValue<Vector2>();
                }

                else
                    _PlayerCircuit.isMoving = false;
                break;
        }
    }
}
