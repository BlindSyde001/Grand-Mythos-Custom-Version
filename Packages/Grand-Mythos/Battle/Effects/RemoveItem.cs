using System;
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

        public void Apply(BattleCharacterController[] targets, EvaluationContext context)
        {
            foreach (var target in targets)
                target.Profile.Inventory.RemoveItem(Item, Amount);
        }

        public string UIDisplayText => $"Remove {((Object)Item)?.name} x {Amount}";
    }
}