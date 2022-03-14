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
                BattleHeroController heroCast = target as BattleHeroController;
                if(BattleStateMachine._HeroesDowned.Contains(heroCast))
                {
                    heroCast.myHero._CurrentHP += (int)(action.PowerModifier * heroCast.myHero.MaxHP);
                    BattleStateMachine._HeroesDowned.Remove(heroCast);
                    BattleStateMachine._HeroesActive.Add(heroCast);
                }
                break;

            case BattleCharacterController.ControllerType.ENEMY:
                BattleEnemyController enemyCast = target as BattleEnemyController;
                if(BattleStateMachine._EnemiesDowned.Contains(enemyCast))
                {
                    enemyCast.myEnemy._CurrentHP += (int)(action.PowerModifier * enemyCast.myEnemy.MaxHP);
                    BattleStateMachine._EnemiesDowned.Remove(enemyCast);
                    BattleStateMachine._EnemiesActive.Add(enemyCast);
                }
                break;
        }
    }
}
