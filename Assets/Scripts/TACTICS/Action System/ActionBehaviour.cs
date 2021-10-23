using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBehaviour : ScriptableObject
{
    public void PreActionTargetting(CharacterCircuit caster, Action action, CharacterCircuit target)
    {
        // Define the Parameters
        // Pull Data from the Caster
        // Compute Stats from Caster and Target with correct Behaviour Equation
        if (action._ActionTargetType == ActionTargetType.MULTI)
        {   // FIND WHERE THE GUY IS IN WHAT LIST => THEN DO IT TO EM ALL
            if (BattleStateMachine._HeroesActive.Contains((HeroExtension)target))
            {
                for (int i = BattleStateMachine._HeroesActive.Count - 1; i >= 0; i--)
                {
                    PerformAction(caster, action, BattleStateMachine._HeroesActive[i]);
                }
            }

            else if (BattleStateMachine._HeroesDowned.Contains((HeroExtension)target))
            {
                for (int i = BattleStateMachine._HeroesDowned.Count - 1; i >= 0; i--)
                {
                    PerformAction(caster, action, BattleStateMachine._HeroesDowned[i]);
                }
            }

            else if (BattleStateMachine._EnemiesActive.Contains((EnemyExtension)target))
            {
                for (int i = BattleStateMachine._EnemiesActive.Count - 1; i >= 0; i--)
                {
                    PerformAction(caster, action, BattleStateMachine._EnemiesActive[i]);
                }
            }

            else if (BattleStateMachine._EnemiesDowned.Contains((EnemyExtension)target))
            {
                for (int i = BattleStateMachine._EnemiesDowned.Count - 1; i >= 0; i--)
                {
                    PerformAction(caster, action, BattleStateMachine._EnemiesDowned[i]);
                }
            }
        }
        else if (action._ActionTargetType == ActionTargetType.SINGLE)
        {
            PerformAction(caster, action, target);
        }
    }
    protected virtual void PerformAction(CharacterCircuit caster, Action action, CharacterCircuit target)
    {

    }
}
