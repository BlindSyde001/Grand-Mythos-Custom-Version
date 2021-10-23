using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[InlineEditor]
public class HeroTacticController : MonoBehaviour
{
    // VARIABLES
    private GameManager GM;

    protected internal HeroExtension myHero;
    public List<Tactic> _TacticsList;

    [SerializeField]
    internal bool ActionIsInputted;     // To check if a player made an action, overwrite current Controller
    internal Action ChosenAction;
    internal CharacterCircuit ChosenTarget;

    // METHODS
    internal void SetNextAction()
    {
        if (!ActionIsInputted)
        {
            if (_TacticsList != null) // Checks: TURNED ON => CONDITION MET => FULL ACTION BAR
                for (int i = 0; i < _TacticsList.Count; i++) // Go Down Gambit list
                {
                    _TacticsList[i]._Performer = myHero;
                    if (_TacticsList[i].isTurnedOn)
                    {
                        TryTacticTargets(i); // Apply condition to targets down the list, until one/none is met
                        if (_TacticsList[i].ConditionIsMet && myHero._ActionChargeAmount == 100)
                        {
                            PerformTacticAction(_TacticsList[i]); // Do all the behaviours on the action
                            myHero.ConsumeActionCharge(); // ATB = 0;
                            ChosenAction = null;
                            ChosenTarget = null;
                        }
                        else if (_TacticsList[i].ConditionIsMet)
                        {
                            ChosenAction = _TacticsList[i]._Action;
                            ChosenTarget = _TacticsList[i]._Target;
                            break;
                        }
                    }
                }
        } else if(ActionIsInputted)
        {

        }
    }
    private void TryTacticTargets(int i)
    {
        // FIND CHARACTER TO INPUT INTO CALLCHECK
        // 1. Find what the target is (HERO or ENEMY)
        // 2. Find appropriate list of targets
        // 3. Select a character
        // 4. Check if CALLCHECK Works with this target
        // 5. Repeat from step 3 until all in list have been tried
        CharacterType x = _TacticsList[i].RetrieveTargetType();
        switch (x)
        {
            case CharacterType.CHARACTER:
                for(int j = 0; j < BattleStateMachine._HeroesActive.Count; j++)
                {
                    _TacticsList[i]._Target = BattleStateMachine._HeroesActive[j];
                    _TacticsList[i].CallCheck();
                    if (_TacticsList[i].ConditionIsMet)
                    {
                        break;
                    }
                }
                break;
            case CharacterType.ENEMY:
                for(int j = 0; j < BattleStateMachine._EnemiesActive.Count; j++)
                {
                    _TacticsList[i]._Target = BattleStateMachine._EnemiesActive[j];
                    _TacticsList[i].CallCheck();
                    if (_TacticsList[i].ConditionIsMet)
                    {
                        break;
                    }
                }
                break;
        }
    }
    internal void PerformTacticAction(Tactic _TacticToPerform)
    {
        foreach (ActionBehaviour aBehaviour in _TacticToPerform._Action._Behaviours)
        {
            aBehaviour.PreActionTargetting(_TacticToPerform._Performer,
                                           _TacticToPerform._Action,
                                           _TacticToPerform._Target);
        }
        _TacticToPerform._Target = null;
    }
}
