using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.StatusHandler
{
    [Serializable]
    [CreateAssetMenu(menuName = "IModifier/ElementalAmmo")]
    public class TauntModifier : ScriptableObject, IModifier
    {
        public float Duration = 2f;

        [PreviewField] public Texture Icon;
        [TextArea] public string Description;

        public ModifierDisplay DisplayPrefab;

        ModifierDisplay IModifier.DisplayPrefab => DisplayPrefab;

        public void ModifyStats(ref Stats stats) { }

        public void ModifyOutgoingDelta(EvaluationContext context, BattleCharacterController target, ref ComputableDamageScaling scaling) { }

        public void ModifyIncomingDelta(EvaluationContext context, BattleCharacterController target, ref ComputableDamageScaling scaling) { }

        public bool Temporary => true;

        public bool IsStillValid(AppliedModifier data, EvaluationContext context)
        {
            if (data.Source is not null && data.Source.CurrentHP <= 0)
                return false;

            return context.CombatTimestamp - data.CreationTimeStamp < Duration;
        }
    }
}