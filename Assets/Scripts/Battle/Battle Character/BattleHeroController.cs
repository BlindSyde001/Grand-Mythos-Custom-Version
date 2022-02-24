using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleHeroController : BattleCharacterController
{
    // VARIABLES
    [SerializeField]
    internal HeroExtension myHero;

    // UPDATES
    private void Update()
    {
        if (eventManager._GameState == GameState.BATTLE)
        {
            switch (BattleStateMachine._CombatState)
            {
                case CombatState.START:
                    anim.Play("Enter Fight");
                    break;

                case CombatState.ACTIVE:
                    break;

                case CombatState.WAIT:
                    anim.Play("Stance");
                    break;

                case CombatState.END:
                    anim.Play("Jump");
                    break;
            }
        }
    }
    private void OnEnable()
    {
        
    }
    private void OnDisable()
    {
        
    }

    // METHODS
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
        }
    }

    internal void PerformManualActionWithAnim()
    {
        // Play Anim
        //yield return new WaitForSeconds(0);
        foreach (ActionBehaviour abehaviour in myHero.myTacticController.ChosenAction._Behaviours)
        {
            abehaviour.PreActionTargetting(this,
                                           myHero.myTacticController.ChosenAction,
                                           myHero.myTacticController.ChosenTarget);
        }
        myHero._ActionChargeAmount = 0;
        myHero.myTacticController.ChosenAction = null;
        myHero.myTacticController.ChosenTarget = null;
        myHero.myTacticController.ActionIsInputted = false;
    }
    internal void PerformTacticWithAnim(Tactic _TacticToPerform)
    {
        // Play Anim Here
        //yield return new WaitForSeconds(0);
        foreach (ActionBehaviour aBehaviour in _TacticToPerform._Action._Behaviours)
        {
            aBehaviour.PreActionTargetting(_TacticToPerform._Performer,
                                           _TacticToPerform._Action,
                                           _TacticToPerform._Target);
        }
        _TacticToPerform._Target = null;
        myHero._ActionChargeAmount = 0;
        myHero.myTacticController.ChosenAction = null;
        myHero.myTacticController.ChosenTarget = null;

    }
}
