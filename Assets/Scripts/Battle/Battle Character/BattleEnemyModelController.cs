using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEnemyModelController : BattleCharacterController
{
    // VARIABLES
    [SerializeField]
    internal CharacterTemplate myEnemy;

    string currentAnimState;

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

    void NewCombatState(CombatState combatState)
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

    void ChangeAnimationState(string newAnimState)
    {
        animator.Play(newAnimState);
        currentAnimState = newAnimState;
    }
}
