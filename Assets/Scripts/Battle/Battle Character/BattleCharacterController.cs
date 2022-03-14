using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public abstract class BattleCharacterController : MonoBehaviour
{
    public enum ControllerType { HERO, ENEMY}

    // VARIABLES
    protected EventManager eventManager;
    [SerializeField]
    internal ControllerType myType;
    [SerializeField]
    internal Animator animator;
    [SerializeField]
    protected internal BattleArenaMovement myMovementController;
    [SerializeField]
    internal Transform myBattlingModel;

    // UPDATES
    private void Start()
    {
        eventManager = EventManager._instance;
    }

    // METHODS
    public virtual void ActiveStateBehaviour()
    {

    }
    public virtual void DieCheck()
    {

    }
}
