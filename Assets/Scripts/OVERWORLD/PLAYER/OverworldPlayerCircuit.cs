using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    internal EventManager EM;

    [Space(20)]
    [SerializeField]
    internal GameState _GameState;

    private Rigidbody rb;
    internal CharacterController cc;
    internal Transform referenceDirection;

    internal bool isMoving;

    // UPDATES
    private void Awake()
    {
        EM = FindObjectOfType<EventManager>();
        _GameState = EM._GameState;
        switch(_GameState)
        {
            case (GameState.OVERWORLD):
                rb = GetComponentInChildren<Rigidbody>();
                cc = GetComponentInChildren<CharacterController>();
                referenceDirection = GameObject.Find("Reference Direction").transform;
                break;
        }
    }

}
