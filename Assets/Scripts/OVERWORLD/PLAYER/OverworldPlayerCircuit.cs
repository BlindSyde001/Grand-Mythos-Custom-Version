using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OverworldPlayerCircuit : MonoBehaviour
{
    // VARIABLES
    [SerializeField]
    internal OverworldPlayerInputNode _InputNode;
    [SerializeField]
    internal OverworldPlayerMoveNode _MoveNode;
    [SerializeField]
    internal OverworldPlayerCollisionNode _CollisionNode;
    [SerializeField]
    internal CharacterController cc;
    [SerializeField]
    internal Transform referenceDirection;
    [SerializeField]
    internal bool isMoving;
    internal Vector2 inputMovement;

    // UPDATES
    private void Awake()
    {
        referenceDirection = GameObject.Find("Reference Direction").transform;
    }

    private void OnEnable()
    {
        EventManager.LoadingBattle += SavePositionalData;
    }
    private void OnDisable()
    {
        EventManager.LoadingBattle -= SavePositionalData;
    }

    //METHODS
    internal void SavePositionalData()
    {
        GameManager._instance._LastKnownPosition = transform.position;
        GameManager._instance._LastKnownRotation = transform.rotation;
    }
    internal void InteractionCheck()
    {

    }
}
