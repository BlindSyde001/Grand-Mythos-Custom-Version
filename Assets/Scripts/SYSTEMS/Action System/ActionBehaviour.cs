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
        {
            BattleStateMachine a = FindObjectOfType<BattleStateMachine>();

            if (a._HeroesActive.Contains((HeroExtension)target))
            {
                foreach (HeroExtension x in a._HeroesActive)
                {
                    PerformAction(caster, action, target);
                }
            }
            else if (a._HeroesDowned.Contains((HeroExtension)target))
            {
                foreach (HeroExtension x in a._HeroesDowned)
                {
                    PerformAction(caster, action, target);
                }

            }
            else if (a._EnemiesActive.Contains((EnemyExtension)target))
            {
                foreach (EnemyExtension x in a._EnemiesActive)
                {
                    PerformAction(caster, action, target);
                }
            }
            else if (a._EnemiesDowned.Contains((EnemyExtension)target))
            {
                foreach (EnemyExtension x in a._EnemiesDowned)
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
