using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEnemyModelController : BattleCharacterController
{
    // VARIABLES
    [SerializeField]
    internal CharacterTemplate myEnemy;
    private string currentAnimState;

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
        switch (combatState)
        {
            case CombatState.START:
                if(myEnemy.CurrentHP > 0)
                    ChangeAnimationState(Battle_EnterFight);
                else
                    ChangeAnimationState(Battle_Die);
                break;

            case CombatState.ACTIVE:
                if (myEnemy.CurrentHP > 0)
                    ChangeAnimationState(Battle_Stance);
                else
                    ChangeAnimationState(Battle_Die);
                break;

            case CombatState.WAIT:
                if (myEnemy.CurrentHP > 0)
                    ChangeAnimationState(Battle_Stance);
                else
                    ChangeAnimationState(Battle_Die);
                break;

            case CombatState.END:
                break;
        }
    }
    private void ChangeAnimationState(string newAnimState)
    {
        animator.Play(newAnimState);
        currentAnimState = newAnimState;
    }
}
