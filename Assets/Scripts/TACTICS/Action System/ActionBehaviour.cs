using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBehaviour : ScriptableObject
{
    public void PreActionTargetting(BattleCharacterController caster, Action action, BattleCharacterController target)
    {
        // Define the Parameters
        // Pull Data from the Caster
        // Compute Stats from Caster and Target with correct Behaviour Equation
        if (action._ActionTargetType == ActionTargetType.MULTI)
        {   // FIND WHERE THE GUY IS IN WHAT LIST => THEN DO IT TO EM ALL

            BattleHeroController a = target as BattleHeroController;
            BattleEnemyController b = target as BattleEnemyController;

            // Check Hero Lists
            if (BattleStateMachine._HeroesActive.Contains(a))
            {
                for (int i = BattleStateMachine._HeroesActive.Count - 1; i >= 0; i--)
                {
                    PerformAction(caster, action, BattleStateMachine._HeroesActive[i]);
                }
            }
            else if (BattleStateMachine._HeroesDowned.Contains(a))
            {
                for (int i = BattleStateMachine._HeroesDowned.Count - 1; i >= 0; i--)
                {
                    PerformAction(caster, action, BattleStateMachine._HeroesDowned[i]);
                }
            }

            // Check Enemy Lists
            else if (BattleStateMachine._EnemiesActive.Contains(b))
            {
                for (int i = BattleStateMachine._EnemiesActive.Count - 1; i >= 0; i--)
                {
                    PerformAction(caster, action, BattleStateMachine._EnemiesActive[i]);
                }
            }
            else if (BattleStateMachine._EnemiesDowned.Contains(b))
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
    protected virtual void PerformAction(BattleCharacterController caster, Action action, BattleCharacterController target)
    {

    }
}
