using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Single Instance Behaviour")]
public class SingleInstanceBehaviour : ActionBehaviour
{
    [TextArea]
    public string description = "Activates a Single Instance of either Damage or Healing.";
    protected override void PerformAction(CharacterTemplate caster, Action action, CharacterTemplate target)
    {
        int amount;
        bool isCrit;
        if (Random.Range(1, 101) <= action.critChance)
        {
            isCrit = true;
        }
        else
        {
            isCrit = false;
        }

        switch (action._ActionEffect)
        {
            case (ActionEffect.DAMAGE):
                //phys / mag stat > variation > ~pierce ? ~ > crit ? / tgt phys / mag defense
                if (!action.isFlatAmount)
                {
                    amount = (int)((action.isMagical ? caster.MagAttack : caster.Attack) *
                                    Random.Range(action.powerModifier, action.powerModifier2) *
                                    (isCrit ? 2.5f : 1));
                }
                else
                {
                    amount = (int)action.powerModifier;
                }

                target._CurrentHP -= amount;
                target._CurrentHP = Mathf.Clamp(target._CurrentHP, 0, target.MaxHP);
                Debug.Log(target.charName + " has taken " + amount + " damage from " + caster.charName);
                //target.DieCheck();
                break;

            case (ActionEffect.HEAL):
                if (!action.isFlatAmount)
                {
                    amount = (int)((action.isMagical ? caster.MagAttack : 1) *
                                    Random.Range(action.powerModifier, action.powerModifier2) *
                                    (isCrit ? 2.5f : 1));
                }
                else
                {
                    amount = (int)action.powerModifier;
                }
                target._CurrentHP += amount;
                target._CurrentHP = Mathf.Clamp(target._CurrentHP, 0, target.MaxHP);
                Debug.Log(target + " has restored " + amount + " health from " + caster);
                break;

            case (ActionEffect.OTHER):
                break;
        }
    }
}
