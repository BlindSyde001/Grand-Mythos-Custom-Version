using System;
using Characters;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StatusHandler
{
    [Serializable]
    [CreateAssetMenu(menuName = "IModifier/ElementalAmmo")]
    public class ElementalAmmo : ScriptableObject, IModifier
    {
        [PreviewField] public Texture Icon;
        [TextArea] public string Description;

        public Element Element;

        public ModifierDisplay DisplayPrefab;

        ModifierDisplay IModifier.DisplayPrefab => DisplayPrefab;

        public void ModifyStats(ref Stats stats) { }

        public void ModifyOutgoingDelta(EvaluationContext context, BattleCharacterController target, ref ComputableDamageScaling scaling)
        {
            scaling.Element = Element;
        }

        public void ModifyIncomingDelta(EvaluationContext context, BattleCharacterController target, ref ComputableDamageScaling scaling)
        {

        }

        public bool Temporary => true;
        public bool DisplayOnRightSide => true;
        public bool IsStillValid(AppliedModifier data, EvaluationContext context) => true;
    }
}