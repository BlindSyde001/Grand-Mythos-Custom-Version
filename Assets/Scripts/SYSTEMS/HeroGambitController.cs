using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
[CreateAssetMenu(menuName = "HGC")]
[InlineEditor]
public class HeroGambitController : ScriptableObject
{
    public HeroExtension _AttachedHero;
    public List<Gambit> _GambitList;

    internal void SetGambitAction()
    {
        // Checks: TURNED ON => CONDITION MET => FULL ACTION BAR
        if(_GambitList != null)
        for (int i = 0; i < _GambitList.Count; i++) // Go Down Gambit list
        {
            _GambitList[i]._Hero = _AttachedHero;
            if (_GambitList[i].isTurnedOn)
            {
                TryGambitTargets(i); // Apply condition to targets down the list, until one/none is met
                if (_GambitList[i].ConditionIsMet && _AttachedHero._ActionChargeAmount == 100)
                {
                    PerformGambitAction(_GambitList[i]);
                    _AttachedHero.ConsumeActionCharge();
                }
                else if(_GambitList[i].ConditionIsMet)
                {
                    break;
                }
            }
        }
    }

    private void TryGambitTargets(int i)
    {
        // FIND CHARACTER TO INPUT INTO CALLCHECK
        // 1. Find what the target is (HERO or ENEMY)
        // 2. Find appropriate list of targets
        // 3. Select a character
        // 4. Check if CALLCHECK Works with this target
        // 5. Repeat from step 3 until all in list have been tried
        CharacterType x = _GambitList[i].RetrieveTargetType();
        switch (x)
        {
            case CharacterType.CHARACTER:
                for(int j = 0; j < BattleStateMachine._HeroesActive.Count; j++)
                {
                    _GambitList[i]._Target = BattleStateMachine._HeroesActive[j];
                    _GambitList[i].CallCheck();
                    if (_GambitList[i].ConditionIsMet)
                    {
                        break;
                    }
                }
                break;
            case CharacterType.ENEMY:
                for(int j = 0; j < BattleStateMachine._EnemiesActive.Count; j++)
                {
                    _GambitList[i]._Target = BattleStateMachine._EnemiesActive[j];
                    _GambitList[i].CallCheck();
                    if (_GambitList[i].ConditionIsMet)
                    {
                        break;
                    }
                }
                break;
        }
    }
    internal void PerformGambitAction(Gambit _GambitToPerform)
    {
        foreach (ActionBehaviour aBehaviour in _GambitToPerform._Action._Behaviours)
        {
            aBehaviour.PreActionTargetting(_GambitToPerform._Hero,
                                           _GambitToPerform._Action,
                                           _GambitToPerform._Target);
        }
        _GambitToPerform._Target = null;
    }
}
