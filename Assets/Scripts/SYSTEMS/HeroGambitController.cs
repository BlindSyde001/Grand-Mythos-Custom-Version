using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroGambitController : MonoBehaviour
{
    public HeroExtension _AttachedHero;
    public List<Gambit> _GambitList;

    public void SetGambitAction()
    {
        if(_GambitList != null)
        for (int i = 0; i < _GambitList.Count; i++) // Go Down Gambit list
        {
            _GambitList[i].CallCheck();
            if (_GambitList[i].ConditionIsMet)      // If Condition is met, Perform Action
            {
                if (_AttachedHero._ActionChargeAmount == 100)
                {
                    PerformGambitAction(_GambitList[i]);
                }
                break;
            }
        }
    }
    private void PerformGambitAction(Gambit _GambitToPerform)
    {
        foreach (ActionBehaviour aBehaviour in _GambitToPerform._Action._Behaviours)
        {
            aBehaviour.PerformAction(_GambitToPerform._Hero,
                                     _GambitToPerform._Action,
                                     _GambitToPerform._Target);
        }
    }
}
