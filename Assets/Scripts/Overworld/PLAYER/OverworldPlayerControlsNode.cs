using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OverworldPlayerControlsNode : MonoBehaviour
{
    // VARIABLES
    [SerializeField]
    private OverworldPlayerCircuit _PlayerCircuit;
    private PlayerControls playerControls;
    private EventManager eventManager;

    private bool isLocked;

    [SerializeField]
    internal Vector3 input;
    [SerializeField]
    private Quaternion targetRotation;
    [SerializeField]
    private Quaternion refQuat;
    [SerializeField]
    private float angle;

    private float turnSpeed = 20f;
    private float velocity = 8f;

    // UPDATES
    private void Awake()
    {
        playerControls = new PlayerControls();
        eventManager = EventManager._instance;
    }
    private void LateUpdate()
    {
        switch(eventManager._GameState)
        {
            case GameState.OVERWORLD:
                if (playerControls.OverworldMap.OpenMenu.IsPressed())
                {
                    isLocked = true;
                    _PlayerCircuit.isMoving = false;
                    return;
                } // Move When Pressing button down
                if (playerControls.OverworldMap.Move.IsPressed() && !isLocked)
                {
                    _PlayerCircuit.isMoving = true;
                    _PlayerCircuit.inputMovement = playerControls.OverworldMap.Move.ReadValue<Vector2>();
                    input.x = _PlayerCircuit.inputMovement.x;
                    input.z = _PlayerCircuit.inputMovement.y;
                }
                else // Stop moving
                {
                    _PlayerCircuit.isMoving = false;
                    input.x = 0;
                    input.z = 0;
                } // Rotate the Player
                if (input.sqrMagnitude >= Mathf.Epsilon)
                {
                    CalculateRotateDirection();
                    RotateThePlayer();
                } else
                {
                    refQuat = _PlayerCircuit.referenceDirection.rotation;
                }
                Move();
                break;

            case GameState.MENU:
                if (playerControls.MenuMap.CloseMenu.IsPressed())
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
    void CalculateRotateDirection()
    {
        input = refQuat * input;
        input.y = 0F;

        if (input.magnitude > 1F)
            input.Normalize();                 // Normalize so movement in all directions is same speed

        angle = Mathf.Atan2(input.x, input.z);
        angle = Mathf.Rad2Deg * angle;
    }
    void RotateThePlayer()
    {
        targetRotation = Quaternion.Euler(0, angle, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }
    void Move()
    {
        _PlayerCircuit.cc.Move(new Vector3(input.x * velocity * Time.deltaTime, 
                                           -1 * velocity * Time.deltaTime, 
                                           input.z * velocity * Time.deltaTime));
    }

    private void InteractionPressed(InputAction.CallbackContext context)
    {
        // Tell PlayerCircuit to Interact
        Debug.Log("Has Pressed");
    }
}
