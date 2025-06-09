using System;
using QTE;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;

namespace Effects
{
    [Serializable]
    public class RemoveItem : IEffect
    {
        [HorizontalGroup, HideLabel, SuffixLabel("x")]
        public BaseItem Item;
        [HorizontalGroup, HideLabel]
        public uint Amount = 1;

        public void Apply(BattleCharacterController[] targets, QTEResult result, EvaluationContext context)
        {
            foreach (var target in targets)
                if (result is QTEResult.Correct or QTEResult.Success)
                    target.Profile.Inventory.RemoveItem(Item, Amount);
        }

        public string UIDisplayText => $"Remove {((Object)Item)?.name} x {Amount}";
    }
}