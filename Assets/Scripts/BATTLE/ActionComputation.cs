using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionComputation : MonoBehaviour
{
    // Order of Operations SENDER (Calc Damage/Heal) -> RECEIVER (Calc reduction/resist) -> RESULT (Damage/Heal Taken)
    // Caster (Damage Variables), Target (Defensive Variables)

    // SENDER DATA
    public void Attack(CharacterCircuit caster, CharacterCircuit target)
    {
        bool crit = true;
        int amount = (int)(caster._Attack * Random.Range(1, 1.5f) * (crit ? 2 : 1));
        DefensiveCalc(target, amount);
    }
    public void Ability()
    {

    }
    public void Magic()
    {

    }
    public void Item()
    {

    }

    // RECEIVER DATA
    public void DefensiveCalc(CharacterCircuit target, int amount)
    {
        int finalCalc = target._Defense - amount;
        DamageOrHeal(target, finalCalc);
    }

    // RESULTS DATA
    public void DamageOrHeal(CharacterCircuit target, int finalCalc)
    {
        target._CurrentHP -= finalCalc;
    }
}
