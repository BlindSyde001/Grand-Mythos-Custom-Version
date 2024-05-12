using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StatusHandler
{
    /// <summary>
    /// Note that the game logic does not expect to have multiple instances of those status at the same time
    /// </summary>
    [CreateAssetMenu(menuName = "IModifier/Generic")]
    public class StatusModifier : ScriptableObject, IModifier
    {
        [PreviewField] public Texture Icon;
        [Multiline] public string Description;
        [ReadOnly, SerializeField] guid _id = Guid.NewGuid();

        public ModifierDisplay DisplayPrefab;

        ModifierDisplay IModifier.DisplayPrefab => DisplayPrefab;

        public void ModifyStats(ref Stats stats) { }

        public void ModifyOutgoingDelta(EvaluationContext context, BattleCharacterController target, ref ComputableDamageScaling scaling)
        {

        }

        public void ModifyIncomingDelta(EvaluationContext context, BattleCharacterController target, ref ComputableDamageScaling scaling)
        {

        }
    }
}