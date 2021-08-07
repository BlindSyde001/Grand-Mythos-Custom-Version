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
                if (_GambitList[i].isTurnedOn)
                {
                    _GambitList[i].CallCheck();
                    if (_GambitList[i].ConditionIsMet)      // If Condition is met, Perform Action
                    {
                        if (_AttachedHero._ActionChargeAmount == 100)
                        {
                            PerformGambitAction(_GambitList[i]);
                            _AttachedHero.ConsumeActionCharge();
                        }
                        break;
                    }
                }
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
    }
}
