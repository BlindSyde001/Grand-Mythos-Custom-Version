using System;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;

namespace Effects
{
    [Serializable]
    public class RemoveItem : IEffect
    {
        [HorizontalGroup, HideLabel, SuffixLabel("x")]
        public required BaseItem Item;
        [HorizontalGroup, HideLabel]
        public required uint Amount = 1;

        public void Apply(CharacterTemplate[] targets, EvaluationContext context)
        {
            foreach (var target in targets)
                target.Inventory.RemoveItem(Item, Amount);
        }

        public string UIDisplayText => $"Remove {((Object)Item)?.name} x {Amount}";
    }
}