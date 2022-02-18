using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OverworldPlayerMoveNode : MonoBehaviour
{
    // VARIABLES
    [SerializeField]
    private OverworldPlayerCircuit _PlayerCircuit;

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
    private void FixedUpdate()
    {
        switch(EventManager._instance._GameState)
        {
            case GameState.OVERWORLD:
                // Move When Pressing button down
                if (_PlayerCircuit.isMoving)
                {
                    input.x = _PlayerCircuit.inputMovement.x;
                    input.z = _PlayerCircuit.inputMovement.y;
                } // Stop moving
                else
                {
                    input.x = 0;
                    input.z = 0;
                }
                Debug.Log("My sqrMag is " + input.sqrMagnitude);
                if (input.sqrMagnitude >= Mathf.Epsilon)
                {
                    calculateDirection();
                    rotate();
                }
                else
                {
                    refQuat = _PlayerCircuit.referenceDirection.rotation;
                }
                Move();
                break;
        }
    }

    // METHODS
    void calculateDirection()
    {
        input = refQuat * input;
        input.y = 0F;

        if (input.magnitude > 1F)
            input.Normalize();                 // Normalize so movement in all directions is same speed

        angle = Mathf.Atan2(input.x, input.z);
        angle = Mathf.Rad2Deg * angle;
    }
    void rotate()
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
}
