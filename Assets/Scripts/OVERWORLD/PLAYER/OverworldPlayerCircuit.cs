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

    [Space(20)]
    [SerializeField]
    internal GameState _GameState;

    private EventManager EM;
    private Rigidbody rb;
    internal CharacterController cc;
    [SerializeField]
    internal Transform referenceDirection;

    internal bool isMoving;
    private void Awake()
    {
        EM = FindObjectOfType<EventManager>();
    }
    // UPDATES
    private void Start()
    {
        switch(EM._GameState)
        {
            case (GameState.OVERWORLD):
                _GameState = GameState.OVERWORLD;
                rb = GetComponentInChildren<Rigidbody>();
                cc = GetComponentInChildren<CharacterController>();
                referenceDirection = GameObject.Find("Reference Direction").transform;
                break;
            case (GameState.CUTSCENE):
                _GameState = GameState.CUTSCENE;
                break;
        }
    }

}
