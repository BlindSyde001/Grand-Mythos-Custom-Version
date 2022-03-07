using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Single Instance Behaviour")]
public class SingleInstanceBehaviour : ActionBehaviour
{
    [TextArea]
    public string description = "Activates a Single Instance of either Damage or Healing.";
    protected override void PerformAction(BattleCharacterController caster, Action action, BattleCharacterController target)
    {
        CharacterTemplate casterStats;
        CharacterTemplate targetStats;
        int amount;
        bool isCrit;
        #region Downscaling Controller types and Stats
        if (caster.myType == BattleCharacterController.ControllerType.HERO)
        {
            BattleHeroController casterDownScale = caster as BattleHeroController;
            casterStats = casterDownScale.myHero;
        }
        else
        {
            BattleEnemyController casterDownScale = caster as BattleEnemyController;
            casterStats = casterDownScale.myEnemy;
        }

        if (target.myType == BattleCharacterController.ControllerType.HERO)
        {
            BattleHeroController targetDownScale = target as BattleHeroController;
            targetStats = targetDownScale.myHero;
        } 
        else
        {
            BattleEnemyController targetDownScale = target as BattleEnemyController;
            targetStats = targetDownScale.myEnemy;
        }
        #endregion
        #region Crit roll
        if (Random.Range(1, 101) <= action.critChance)
        {
            isCrit = true;
        }
        else
        {
            isCrit = false;
        }
        #endregion

        switch (action.ActionEffect)
        {
            case ActionEffect.DAMAGE:
                //phys / mag stat > variation > ~pierce ? ~ > crit ? / tgt phys / mag defense
                if (!action.isFlatAmount)
                {
                    amount = (int)((action.isMagical ? casterStats.MagAttack : casterStats.Attack) *
                                    Random.Range(action.PowerModifier, action.PowerModifier2) *
                                    (isCrit ? 2.5f : 1));
                }
                else
                {
                    amount = (int)action.PowerModifier;
                }

                if (target != null)
                {
                    targetStats._CurrentHP -= amount;
                    targetStats._CurrentHP = Mathf.Clamp(targetStats._CurrentHP, 0, targetStats.MaxHP);
                    //Debug.Log(targetStats.name + " has taken " + amount + " damage from " + casterStats.charName);
                    target.DieCheck();
                }
                break;

            case (ActionEffect.HEAL):
                if (!action.isFlatAmount)
                {
                    amount = (int)((action.isMagical ? casterStats.MagAttack : 1) *
                                    Random.Range(action.PowerModifier, action.PowerModifier2) *
                                    (isCrit ? 2.5f : 1));
                }
                else
                {
                    amount = (int)action.PowerModifier;
                }
                if (target != null)
                {
                    targetStats._CurrentHP += amount;
                    targetStats._CurrentHP = Mathf.Clamp(targetStats._CurrentHP, 0, targetStats.MaxHP);
                    //Debug.Log(target + " has restored " + amount + " health from " + caster);
                }
                break;

            case (ActionEffect.OTHER):
                break;
        }
    }
}
