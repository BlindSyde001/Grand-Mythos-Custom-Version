using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleHeroController : BattleCharacterController
{
    // VARIABLES
    [SerializeField]
    internal HeroExtension myHero;
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
    internal IEnumerator PerformManualActionWithAnim()
    {
        // Play Anim
        if (myHero._CurrentHP > 0)
        {
            myHero._ActionChargeAmount = 0;
            ChangeAnimationState(myHero.myTacticController.ChosenAction.AnimationName);
            yield return new WaitForSeconds(myHero.myTacticController.ChosenAction.AnimationTiming);

            foreach (ActionBehaviour abehaviour in myHero.myTacticController.ChosenAction.Behaviours)
            {
                abehaviour.PreActionTargetting(this,
                                               myHero.myTacticController.ChosenAction,
                                               myHero.myTacticController.ChosenTarget);
            }
        }
        else
        {
            yield return null;
        }
        myHero.myTacticController.ChosenAction = null;
        myHero.myTacticController.ChosenTarget = null;
        myHero.myTacticController.ActionIsInputted = false;
    }
    internal IEnumerator PerformTacticWithAnim(Tactic _TacticToPerform)
    {
        // Play Anim Here
        if (myHero._CurrentHP > 0)
        {
            myHero._ActionChargeAmount = 0;
            ChangeAnimationState(myHero.myTacticController.ChosenAction.AnimationName);
            yield return new WaitForSeconds(myHero.myTacticController.ChosenAction.AnimationTiming);

            foreach (ActionBehaviour aBehaviour in _TacticToPerform._Action.Behaviours)
            {
                aBehaviour.PreActionTargetting(_TacticToPerform._Performer,
                                               _TacticToPerform._Action,
                                               _TacticToPerform._Target);
            }
        }
        else
        {
            yield return null;
        }
        _TacticToPerform._Target = null;
        myHero.myTacticController.ChosenAction = null;
        myHero.myTacticController.ChosenTarget = null;
    }

    public override void ActiveStateBehaviour()
    {
        myHero._ActionChargeAmount += myHero._ActionRechargeSpeed * Time.deltaTime;
        myHero._ActionChargeAmount = Mathf.Clamp(myHero._ActionChargeAmount, 0, 100);
        myHero.myTacticController.SetNextAction();
    }
    public override void DieCheck()
    {
        if (myHero._CurrentHP <= 0)
        {
            myHero._CurrentHP = 0;
            myHero._ActionChargeAmount = 0;
            FindObjectOfType<BattleStateMachine>().CheckCharIsDead(this);
            ChangeAnimationState(Battle_Die);
        }
    }
    #endregion

    private void NewCombatState(CombatState combatState)
    {
        switch(combatState)
        {
            case CombatState.START:
                if (myHero._CurrentHP > 0)
                {
                    ChangeAnimationState(Battle_EnterFight);
                }
                else
                {
                    ChangeAnimationState(Battle_Die);
                }
                break;

            case CombatState.ACTIVE:
                if (myHero._CurrentHP > 0)
                {
                    ChangeAnimationState(Battle_Stance);
                    myMovementController.isRoaming = true;
                }
                break;

            case CombatState.WAIT:
                if (myHero._CurrentHP > 0)
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
