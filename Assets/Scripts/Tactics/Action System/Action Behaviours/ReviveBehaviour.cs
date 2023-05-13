using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Revive Behaviour")]
public class ReviveBehaviour : ActionBehaviour
{
    [TextArea]
    public string description = "Revives Party Member in Battle";

    protected override void PerformAction(BattleCharacterController caster, Action action, BattleCharacterController target)
    {
        switch (target.myType)
        {
            case BattleCharacterController.ControllerType.HERO:

                BattleHeroModelController heroCast = target as BattleHeroModelController;

                if(BattleStateMachine._HeroesDowned.Contains(heroCast) && !heroCast.isAlive)
                {
                    heroCast.myHero._CurrentHP += (int)(action.PowerModifier/100 * heroCast.myHero.MaxHP);
                    heroCast.HasRevived();
                }
                break;

            case BattleCharacterController.ControllerType.ENEMY:
                
                BattleEnemyModelController enemyCast = target as BattleEnemyModelController;

                if(BattleStateMachine._EnemiesDowned.Contains(enemyCast) && !enemyCast.isAlive)
                {
                    enemyCast.myEnemy._CurrentHP += (int)(action.PowerModifier/100 * enemyCast.myEnemy.MaxHP);
                    BattleStateMachine._EnemiesDowned.Remove(enemyCast);
                    BattleStateMachine._EnemiesActive.Add(enemyCast);
                }
                break;
        }
    }
}
