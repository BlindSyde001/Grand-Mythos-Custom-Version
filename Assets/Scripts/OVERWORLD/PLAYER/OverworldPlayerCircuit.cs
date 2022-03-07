using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OverworldPlayerCircuit : MonoBehaviour
{
    // VARIABLES
    [SerializeField]
    internal OverworldPlayerControlsNode _MoveNode;
    [SerializeField]
    internal OverworldPlayerCollisionNode _CollisionNode;
    [SerializeField]
    internal CharacterController cc;
    [SerializeField]
    internal bool isMoving;
    internal Vector2 inputMovement;
    [SerializeField]
    internal Transform referenceDirection;

    // UPDATES
    private void Awake()
    {
        referenceDirection = FindObjectOfType<CameraManager>().ReferenceDirections[0];
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
        GameManager._instance.LastKnownPosition = transform.position;
        GameManager._instance.LastKnownRotation = transform.rotation;
    }
    internal void InteractionCheck()
    {

    }
}
