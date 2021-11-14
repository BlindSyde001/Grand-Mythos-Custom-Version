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

    private Rigidbody rb;
    internal CharacterController cc;
    [SerializeField]
    internal Transform referenceDirection;

    internal bool isMoving;
    // UPDATES
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
        _InputNode = GetComponent<OverworldPlayerInputNode>();
        _MoveNode = GetComponent<OverworldPlayerMoveNode>();
        _CollisionNode = GetComponent<OverworldPlayerCollisionNode>();

        referenceDirection = GameObject.Find("Reference Direction").transform;
    }
    private void Start()
    {
        switch(EventManager._instance._GameState)
        {
            case (GameState.OVERWORLD):
                _GameState = GameState.OVERWORLD;
                break;
            case (GameState.CUTSCENE):
                _GameState = GameState.CUTSCENE;
                break;
        }
    }
    private void OnEnable()
    {
        EventManager.ChangeToBattleState += SavePositionalData;
    }
    private void OnDisable()
    {
        EventManager.ChangeToBattleState -= SavePositionalData;
    }
    //METHODS
    internal void SavePositionalData(GameState GS)
    {
        if (GS == GameState.BATTLE)
        {
            GameManager._instance._LastKnownPosition = this.transform.position;
            GameManager._instance._LastKnownRotation = this.transform.rotation;
        }
    }
}
