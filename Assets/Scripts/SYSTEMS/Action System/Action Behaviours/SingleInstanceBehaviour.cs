using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Single Instance Behaviour")]
public class SingleInstanceBehaviour : ActionBehaviour
{
    [TextArea]
    public string description = "Activates a Single Instance of either Damage or Healing.";
    public override void PerformAction(CharacterCircuit caster, Action action, CharacterCircuit target)
    {
        // Define the Parameters
        // Pull Data from the Caster
        // Compute Stats from Caster and Target with correct Behaviour Equation
        int amount;
        bool isCrit;
        if (Random.Range(1, 101) > action.critChance)
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
                amount = (int)((action.isMagical ? caster.MagAttack : caster.Attack) *
                                (Random.Range(action.powerModifier, action.powerModifier2)) *
                                (isCrit ? 2.5f : 1)
                                / (action.isMagical ? target.MagDefense : target.Defense));
                target._CurrentHP -= amount;
                Debug.Log(target + " has taken " + amount + " damage from " + caster);
                Debug.Log(target._CurrentHP + "/" + target.MaxHP);
                break;

            case (ActionEffect.HEAL):
                break;

            case (ActionEffect.OTHER):
                break;
        }
    }
}
