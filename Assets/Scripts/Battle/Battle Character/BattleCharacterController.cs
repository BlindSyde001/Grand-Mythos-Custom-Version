using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class BattleCharacterController : MonoBehaviour
{
    public CharacterTemplate Profile;

    [Required]
    public Animator Animator;
    [SerializeField, Required]
    protected BattleArenaMovement MovementController;

    [NonSerialized]
    public EvaluationContext Context;

    const string Battle_EnterFight = "Enter Fight";
    const string Battle_Stance = "Stance";
    const string Battle_Die = "Die";

    public bool IsHostileTo(BattleCharacterController character)
    {
        return Profile.Team.Allies.Contains(character.Profile.Team) == false;
    }

    public BattleCharacterController()
    {
        Context = new(this);
    }

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
            bts.Include(this);
    }

    void OnDestroy()
    {
        if (BattleStateMachine.TryGetInstance(out var bts))
            bts.Exclude(this);
    }

    void OnDrawGizmosSelected()
    {
        this.AutoAssign(ref Animator);
        this.AutoAssign(ref MovementController);
    }

    void NewCombatState(CombatState combatState)
    {
        if (Profile.CurrentHP <= 0)
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
