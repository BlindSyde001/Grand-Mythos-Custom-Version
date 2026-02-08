using System;

namespace StatusHandler
{
    [Serializable]
    public class PhysicalDamageMultiplier : IStatusModifierLogic
    {
        public float Multiplier = 1.5f;
        public void Modify(EvaluationContext context, CharacterTemplate target, ref ComputableDamageScaling scaling)
        {
            if (scaling.Scaling == ComputableDamageScaling.ScalingType.Physical)
                scaling.BaseValue *= Multiplier;
        }
    }
}