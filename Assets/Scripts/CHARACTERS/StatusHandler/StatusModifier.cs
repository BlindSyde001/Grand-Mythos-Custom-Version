using Sirenix.OdinInspector;
using UnityEngine;

namespace StatusHandler
{
    /// <summary>
    /// Note that the game logic does not expect to have multiple instances of those status at the same time
    /// </summary>
    [CreateAssetMenu(menuName = "IModifier/Generic")]
    public class StatusModifier : IdentifiableScriptableObject, IModifier
    {
        private const string IncomingDescription = "What happens to attacks that this unit receives";
        private const string OutgoingDescription = "What happens to attacks that this unit deals";
        
        [PreviewField] public Sprite Icon;
        [TextArea] public string Description;

        [Required] public ModifierDisplay DisplayPrefab;

        [SerializeReference, InfoBox(OutgoingDescription), BoxGroup(nameof(Outgoing)), HideLabel] public IStatusModifierLogic Outgoing;
        [SerializeReference, InfoBox(IncomingDescription), BoxGroup(nameof(Incoming)), HideLabel] public IStatusModifierLogic Incoming;

        ModifierDisplay IModifier.DisplayPrefab => DisplayPrefab;

        public void ModifyStats(ref Stats stats) { }

        public void ModifyOutgoingDelta(EvaluationContext context, BattleCharacterController target, ref ComputableDamageScaling scaling)
        {
            Outgoing?.Modify(context, target, ref scaling);
        }

        public void ModifyIncomingDelta(EvaluationContext context, BattleCharacterController target, ref ComputableDamageScaling scaling)
        {
            Incoming?.Modify(context, target, ref scaling);
        }
    }
}