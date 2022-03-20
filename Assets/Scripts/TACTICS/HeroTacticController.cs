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
    internal bool ManualActionInput;                        // To check if a player made an action, overwrite current Controller
    internal List<Action> ChosenActions;
    [SerializeField]
    internal BattleCharacterController ChosenTarget;
    [SerializeField]
    internal int ChosenTargetList = 0;
    internal int ActionSegments = 4;

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
        foreach(Action action in _TacticsList[i]._Actions)
        {
            if (action != null)
            {
                if (action.ActionType == ActionType.ITEM)
                {
                    Consumable consumable = GameManager._instance._ConsumablesDatabase.Find(x => x.myAction == action);
                    ItemCapsule itemCapsule = InventoryManager._instance.ConsumablesInBag.Find(x => x.thisItem == consumable);
                    if (itemCapsule == null)
                    {
                        _TacticsList[i].ConditionIsMet = false;
                        return;
                    }
                }
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
                            ChosenTargetList = 1;
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
                            ChosenTargetList = 2;
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
                            ChosenTargetList = 3;
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
                            ChosenTargetList = 4;
                            break;
                        }
                    }
                }
                break;
        }
    }
    internal bool CheckIfStillInList(BattleCharacterController currentTarget, int targetList)
    {
        switch (targetList)
        {
            case 1:
                if (BattleStateMachine._HeroesActive.Contains(currentTarget as BattleHeroController))
                {
                    //Debug.Log("1. checking for Target: " + currentTarget);
                    return true;
                }
                else
                {
                    return false;
                }

            case 2:
                if (BattleStateMachine._HeroesDowned.Contains(currentTarget as BattleHeroController))
                {
                    //Debug.Log("1. checking for Target: " + currentTarget);
                    return true;
                }
                else
                {
                    return false;
                }

            case 3:
                if (BattleStateMachine._EnemiesActive.Contains(currentTarget as BattleEnemyController))
                {
                    //Debug.Log("1. checking for Target: " + currentTarget);
                    return true;
                }
                else
                {
                    return false;
                }

            case 4:
                if (BattleStateMachine._EnemiesDowned.Contains(currentTarget as BattleEnemyController))
                {
                    //Debug.Log("1. checking for Target: " + currentTarget);
                    return true;
                }
                else
                {
                    return false;
                }
        }
        return false;
    }
    internal void SetNextAction()
    {
        // Automated AI Tactic
        if (!ManualActionInput)
        {
            if (_TacticsList != null) // Checks: TURNED ON => CONDITION MET => (ITEM) HAS ENOUGH IN INVENTORY => FULL ACTION BAR
            {
                for (int i = 0; i < _TacticsList.Count; i++) // Go Down Gambit list
                {
                    _TacticsList[i]._Performer = myHeroCtrlr;
                    if (_TacticsList[i].isTurnedOn) // > IS TURNED ON?
                    {
                        TryTacticTargets(i); // > IS TARGET ELIGIBLE
                        if (_TacticsList[i].ConditionIsMet && myHeroCtrlr.myHero._ActionChargeAmount == 100) // IS ACTION BAR FULL?
                        {
                            StartCoroutine(myHeroCtrlr.myHero.myBattleHeroController.DoTacticAction(_TacticsList[i], ChosenTargetList)); // Do all the Actions & Behaviours on the action
                        }
                        else if (_TacticsList[i].ConditionIsMet)
                        {
                            ChosenActions = _TacticsList[i]._Actions;
                            ChosenTarget = _TacticsList[i]._Target;
                            myHeroCtrlr.myMovementController.myTarget = ChosenTarget.myBattlingModel;
                            break;
                        }
                    }
                }
            }
        } 
        // Manual Command
        else if(ManualActionInput)
        {
            if(ChosenTarget != null || ChosenTarget)
            {
                if (myHeroCtrlr.myHero._ActionChargeAmount == 100)
                {
                    StartCoroutine(myHeroCtrlr.myHero.myBattleHeroController.DoManualAction(ChosenActions, ChosenTarget, ChosenTargetList));
                }
                else
                {
                    myHeroCtrlr.myMovementController.myTarget = ChosenTarget.myBattlingModel;
                }
            }
            else
            {
                ManualActionInput = false;
                return;
            }
        }
    }
}