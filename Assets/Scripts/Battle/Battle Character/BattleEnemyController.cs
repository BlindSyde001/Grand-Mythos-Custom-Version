using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEnemyController : BattleCharacterController
{
    // VARIABLES
    [SerializeField]
    internal EnemyExtension myEnemy;
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

    // METHODS
    #region Standard Action Methods
    internal IEnumerator PerformEnemyAction(Action action, BattleCharacterController target)
    {
        if (myEnemy._CurrentHP > 0)
        {
            myEnemy._ActionChargeAmount = 0;
            ChangeAnimationState(action.AnimationName);
            yield return new WaitForSeconds(action.AnimationTiming);
            foreach (ActionBehaviour aBehaviour in action.Behaviours)
            {
                aBehaviour.PreActionTargetting(myEnemy.myBattleEnemyController, action, target);
            }
        }
        else
        {
            yield return null;
        }
    }


    public override void ActiveStateBehaviour()
    {
        if (myEnemy._CurrentHP > 0)
        {
            myEnemy._ActionChargeAmount += myEnemy._ActionRechargeSpeed * Time.deltaTime;
            myEnemy._ActionChargeAmount = Mathf.Clamp(myEnemy._ActionChargeAmount, 0, 100);
            myEnemy.EnemyAct();
        }
    }
    public override void DieCheck()
    {
        if (myEnemy._CurrentHP <= 0)
        {
            myEnemy._CurrentHP = 0;
            myEnemy._ActionChargeAmount = 0;
            FindObjectOfType<BattleStateMachine>().CheckCharIsDead(this);
            ChangeAnimationState(Battle_Die);
        }
    }
    #endregion

    private void NewCombatState(CombatState combatState)
    {
        switch (combatState)
        {
            case CombatState.START:
                if(myEnemy._CurrentHP > 0)
                ChangeAnimationState(Battle_EnterFight);
                break;

            case CombatState.ACTIVE:
                if (myEnemy._CurrentHP > 0)
                    ChangeAnimationState(Battle_Stance);
                break;

            case CombatState.WAIT:
                if (myEnemy._CurrentHP > 0)
                    ChangeAnimationState(Battle_Stance);
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
