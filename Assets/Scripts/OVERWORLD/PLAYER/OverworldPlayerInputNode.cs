using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OverworldPlayerInputNode : MonoBehaviour
{
    // VARIABLES
    [SerializeField]
    private OverworldPlayerCircuit _PlayerCircuit;
    private InputManager inputManager;
    private EventManager eventManager;

    // UPDATES
    private void Start()
    {
        inputManager = InputManager._instance;
        eventManager = EventManager._instance;
    }
    private void Update()
    {
        switch(eventManager._GameState)
        {
            case GameState.OVERWORLD:

                if (inputManager.playerInput.actions.FindAction("Move").IsPressed())
                {
                    _PlayerCircuit.isMoving = true;
                    _PlayerCircuit.inputMovement = inputManager.playerInput.actions.FindAction("Move").ReadValue<Vector2>();
                    Debug.Log("Circuit Input Movement :" + _PlayerCircuit.inputMovement);
                }

                else
                    _PlayerCircuit.isMoving = false;
                break;
        }
    }
}
