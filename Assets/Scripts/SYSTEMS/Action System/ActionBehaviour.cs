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
                foreach (HeroExtension x in BattleStateMachine._HeroesActive)
                {
                    PerformAction(caster, action, target);
                }
            }
            else if (BattleStateMachine._HeroesDowned.Contains((HeroExtension)target))
            {
                foreach (HeroExtension x in BattleStateMachine._HeroesDowned)
                {
                    PerformAction(caster, action, target);
                }

            }
            else if (BattleStateMachine._EnemiesActive.Contains((EnemyExtension)target))
            {
                foreach (EnemyExtension x in BattleStateMachine._EnemiesActive)
                {
                    PerformAction(caster, action, target);
                }
            }
            else if (BattleStateMachine._EnemiesDowned.Contains((EnemyExtension)target))
            {
                foreach (EnemyExtension x in BattleStateMachine._EnemiesDowned)
                {
                    PerformAction(caster, action, target);
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
