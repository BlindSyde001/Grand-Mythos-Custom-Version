using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleHeroModelController : BattleCharacterController
{
    // VARIABLES
    [SerializeField]
    internal HeroExtension myHero;
    private string currentAnimState;
    internal bool isPerformingActions;

    const string Battle_EnterFight = "Enter Fight";
    const string Battle_Stance = "Stance";
    const string Battle_Die = "Die";

    // UPDATES
    private void OnEnable()
    {
        BattleStateMachine.OnNewStateSwitched += NewCombatState;
    }
    private void OnDisable()
    {
        BattleStateMachine.OnNewStateSwitched -= NewCombatState;
    }

    private void NewCombatState(CombatState combatState)
    {
        switch(combatState)
        {
            case CombatState.START:
                if (myHero.CurrentHP > 0)
                {
                    ChangeAnimationState(Battle_EnterFight);
                }
                else
                {
                    ChangeAnimationState(Battle_Die);
                }
                break;

            case CombatState.ACTIVE:
                if (myHero.CurrentHP > 0)
                {
                    ChangeAnimationState(Battle_Stance);
                    myMovementController.isRoaming = true;
                }
                else
                {
                    myMovementController.isRoaming = false;
                }
                break;

            case CombatState.WAIT:
                if (myHero.CurrentHP > 0)
                {
                    ChangeAnimationState(Battle_Stance);
                    myMovementController.isRoaming = false;
                }
                break;

            case CombatState.END:
                myMovementController.isRoaming = false;
                break;
        }
    }
    private void ChangeAnimationState(string newAnimState)
    {
        animator.Play(newAnimState);
        currentAnimState = newAnimState;
    }
}
