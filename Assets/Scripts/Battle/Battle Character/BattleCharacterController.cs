using UnityEngine;

public abstract class BattleCharacterController : MonoBehaviour
{
    [SerializeField]
    internal Animator animator;
    [SerializeField]
    protected internal BattleArenaMovement myMovementController;
}
