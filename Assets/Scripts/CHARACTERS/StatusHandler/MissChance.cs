using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StatusHandler
{
    [Serializable]
    public class MissChance : IStatusModifierLogic
    {
        [Range(0,100), SuffixLabel("%")]
        public float Chance = 50f;
        
        public void Modify(EvaluationContext context, BattleCharacterController target, ref ComputableDamageScaling scaling)
        {
            if (context.Random.NextFloat(0, 100) < Chance)
            {
                scaling.BaseValue = 0f;
                scaling.VarianceRolled = 0;
            }
        }
    }
}