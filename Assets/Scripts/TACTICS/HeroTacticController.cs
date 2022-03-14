using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[InlineEditor]
public class HeroTacticController : MonoBehaviour
{
    // VARIABLES
    [SerializeField]
    internal BattleHeroController myHeroCtrlr;
    public List<Tactic> _TacticsList;

    [SerializeField]
    internal bool ActionIsInputted;     // To check if a player made an action, overwrite current Controller
    internal Action ChosenAction;
    internal BattleCharacterController ChosenTarget;

    // METHODS
    private void TryTacticTargets(int i)
    {
        // FIND CHARACTER TO INPUT INTO CALLCHECK
        // 1. Find what the target is (HERO or ENEMY)
        // 2. Find appropriate list of targets
        // 3. Select a character
        // 4. Check if CALLCHECK Works with this target
        // 5. Repeat from step 3 until all in list have been tried
        CharacterType x = _TacticsList[i].RetrieveTargetType();
        if (_TacticsList[i]._Action.ActionType == ActionType.ITEM)
        {
            Consumable consumable = GameManager._instance._ConsumablesDatabase.Find(x => x.myAction == _TacticsList[i]._Action);
            ItemCapsule itemCapsule = InventoryManager._instance.ConsumablesInBag.Find(x => x.thisItem == consumable);
            if(itemCapsule == null)
            {
                _TacticsList[i].ConditionIsMet = false;
                return;
            }
        }
        switch (x)
        {
            case CharacterType.CHARACTER:
                if (_TacticsList[i].RetrieveTargetStatus() == CharacterActiveStatus.ACTIVE)
                {
                    if(BattleStateMachine._HeroesActive.Count == 0)
                    {
                        _TacticsList[i].ConditionIsMet = false;
                        break;
                    }
                    for (int j = 0; j < BattleStateMachine._HeroesActive.Count; j++)
                    {
                        _TacticsList[i]._Target = BattleStateMachine._HeroesActive[j];
                        _TacticsList[i].CallCheck();
                        if (_TacticsList[i].ConditionIsMet)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    if (BattleStateMachine._HeroesDowned.Count == 0)
                    {
                        _TacticsList[i].ConditionIsMet = false;
                        break;
                    }
                    for (int j = 0; j < BattleStateMachine._HeroesDowned.Count; j++)
                    {
                        _TacticsList[i]._Target = BattleStateMachine._HeroesDowned[j];
                        _TacticsList[i].CallCheck();
                        if (_TacticsList[i].ConditionIsMet)
                        {
                            break;
                        }
                    }
                }
                break;

            case CharacterType.ENEMY:
                if (_TacticsList[i].RetrieveTargetStatus() == CharacterActiveStatus.ACTIVE)
                {
                    if (BattleStateMachine._EnemiesActive.Count == 0)
                    {
                        _TacticsList[i].ConditionIsMet = false;
                        break;
                    }
                    for (int j = 0; j < BattleStateMachine._EnemiesActive.Count; j++)
                    {
                        _TacticsList[i]._Target = BattleStateMachine._EnemiesActive[j];
                        _TacticsList[i].CallCheck();
                        if (_TacticsList[i].ConditionIsMet)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    if (BattleStateMachine._EnemiesDowned.Count == 0)
                    {
                        _TacticsList[i].ConditionIsMet = false;
                        break;
                    }
                    for (int j = 0; j < BattleStateMachine._EnemiesDowned.Count; j++)
                    {
                        _TacticsList[i]._Target = BattleStateMachine._EnemiesDowned[j];
                        _TacticsList[i].CallCheck();
                        if (_TacticsList[i].ConditionIsMet)
                        {
                            break;
                        }
                    }
                }
                break;
        }
    }
    internal void SetNextAction()
    {
        if (!ActionIsInputted) // AI Behaviours
        {
            if (_TacticsList != null) // Checks: TURNED ON => CONDITION MET => (ITEM) HAS ENOUGH IN INVENTORY => FULL ACTION BAR
            {
                for (int i = 0; i < _TacticsList.Count; i++) // Go Down Gambit list
                {
                    _TacticsList[i]._Performer = myHeroCtrlr;
                    if (_TacticsList[i].isTurnedOn)
                    {
                        TryTacticTargets(i); // Apply condition to targets down the list, until one/none is met
                        if (_TacticsList[i].ConditionIsMet && myHeroCtrlr.myHero._ActionChargeAmount == 100)
                        {
                            StartCoroutine(myHeroCtrlr.myHero.myBattleHeroController.PerformTacticWithAnim(_TacticsList[i])); // Do all the behaviours on the action
                        }
                        else if (_TacticsList[i].ConditionIsMet)
                        {
                            ChosenAction = _TacticsList[i]._Action;
                            ChosenTarget = _TacticsList[i]._Target;
                            myHeroCtrlr.myMovementController.myTarget = ChosenTarget.myBattlingModel;
                            break;
                        }
                    }
                }
            }
        } 
        else if(ActionIsInputted) // Manual Command
        {
            if(ChosenTarget != null || ChosenTarget)
            {
                if (myHeroCtrlr.myHero._ActionChargeAmount == 100)
                {
                    StartCoroutine(myHeroCtrlr.myHero.myBattleHeroController.PerformManualActionWithAnim());
                }
                else
                {
                    myHeroCtrlr.myMovementController.myTarget = ChosenTarget.myBattlingModel;
                }
            }
            else
            {
                ActionIsInputted = false;
                return;
            }
        }
    }
}
