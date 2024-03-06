using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class BattleCharacterController : MonoBehaviour
{
    public CharacterTemplate Template;

    [Required]
    public Animator Animator;
    [SerializeField, Required]
    protected BattleArenaMovement MovementController;

    const string Battle_EnterFight = "Enter Fight";
    const string Battle_Stance = "Stance";
    const string Battle_Die = "Die";

    // UPDATES
    void OnEnable()
    {
        BattleStateMachine.OnNewStateSwitched += NewCombatState;
    }

    void OnDisable()
    {
        BattleStateMachine.OnNewStateSwitched -= NewCombatState;
    }

    void Start()
    {
        if (BattleStateMachine.TryGetInstance(out var bts))
            bts.Include(Template);
        Template.Context.Controller = this;
    }

    void OnDestroy()
    {
        if (BattleStateMachine.TryGetInstance(out var bts))
            bts.Exclude(Template);
        Template.Context.Controller = null;
    }

    void OnDrawGizmosSelected()
    {
        this.AutoAssign(ref Animator);
        this.AutoAssign(ref MovementController);
    }

    void NewCombatState(CombatState combatState)
    {
        if (Template.CurrentHP <= 0)
        {
            ChangeAnimationState(Battle_Die);
            return;
        }

        switch (combatState)
        {
            case CombatState.Start:
                ChangeAnimationState(Battle_EnterFight);
                break;

            case CombatState.Active:
                ChangeAnimationState(Battle_Stance);
                break;

            case CombatState.Wait:
                ChangeAnimationState(Battle_Stance);
                break;

            case CombatState.End:
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(combatState), combatState, null);
        }
    }

    void ChangeAnimationState(string newAnimState)
    {
        Animator.Play(newAnimState);
    }
}
