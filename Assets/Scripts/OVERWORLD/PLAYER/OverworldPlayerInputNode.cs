using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OverworldPlayerInputNode : MonoBehaviour
{
    // VARIABLES
    [SerializeField]
    private OverworldPlayerCircuit _PlayerCircuit;
    private EventManager eventManager;

    private bool isLocked;

    [SerializeField]
    private PlayerControls playerControls;

    // UPDATES
    private void Awake()
    {
        playerControls = new PlayerControls();
        eventManager = EventManager._instance;
    }
    private void Update()
    {
        switch (eventManager._GameState)
        {
            case GameState.OVERWORLD:
                if(playerControls.OverworldMap.OpenMenu.IsPressed())
                {
                    isLocked = true;
                    _PlayerCircuit.isMoving = false;
                    return;
                }
                if (playerControls.OverworldMap.Move.IsPressed() && !isLocked)
                {
                    _PlayerCircuit.isMoving = true;
                    _PlayerCircuit.inputMovement = playerControls.OverworldMap.Move.ReadValue<Vector2>();
                }
                else
                {
                    _PlayerCircuit.isMoving = false;
                }
                break;

            case GameState.MENU:
                if(playerControls.MenuMap.CloseMenu.IsPressed())
                {
                    isLocked = false;
                }
                break;
        }
    }
    private void OnEnable()
    {
        playerControls.Enable();
        playerControls.OverworldMap.Interact.performed += InteractionPressed;
    }
    private void OnDisable()
    {
        playerControls.Disable();
        playerControls.OverworldMap.Interact.performed -= InteractionPressed;
    }

    // METHODS
    private void InteractionPressed(InputAction.CallbackContext context)
    {
        // Tell PlayerCircuit to Interact
        Debug.Log("Has Pressed");
    }
}
