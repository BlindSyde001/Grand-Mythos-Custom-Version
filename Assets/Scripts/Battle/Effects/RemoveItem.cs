using System;
using System.Collections;
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

        public IEnumerable Apply(TargetCollection targets, EvaluationContext context)
        {
            foreach (var target in targets)
                target.Inventory.RemoveItem(Item, Amount);
            yield break;
        }

        public string UIDisplayText => $"Remove {((Object)Item)?.name} x {Amount}";
    }
}