using Characters;
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
        
        [TextArea] public string Description = "";

        public required ModifierDisplay DisplayPrefab;

        [SerializeReference, InfoBox(OutgoingDescription), BoxGroup(nameof(Outgoing)), HideLabel] public IStatusModifierLogic? Outgoing;
        [SerializeReference, InfoBox(IncomingDescription), BoxGroup(nameof(Incoming)), HideLabel] public IStatusModifierLogic? Incoming;

        [InfoBox("Is this status only effective during the encounter were it was applied, or carried between each encounter")]
        public bool Temporary = true;

        public float Duration = float.PositiveInfinity;

        ModifierDisplay? IModifier.DisplayPrefab => DisplayPrefab;

        public void ModifyStats(ref Stats stats) { }

        public void ModifyOutgoingDelta(EvaluationContext context, CharacterTemplate target, ref ComputableDamageScaling scaling)
        {
            Outgoing?.Modify(context, target, ref scaling);
        }

        public void ModifyIncomingDelta(EvaluationContext context, CharacterTemplate target, ref ComputableDamageScaling scaling)
        {
            Incoming?.Modify(context, target, ref scaling);
        }

        bool IModifier.Temporary => Temporary;
        public bool DisplayOnRightSide => false;

        public bool IsStillValid(AppliedModifier data, EvaluationContext context)
        {
            return context.CombatTimestamp - data.CreationTimeStamp < Duration;
        }
    }
}