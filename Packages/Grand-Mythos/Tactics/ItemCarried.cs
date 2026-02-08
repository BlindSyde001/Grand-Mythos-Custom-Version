using System;
using System.Diagnostics.CodeAnalysis;
using Sirenix.OdinInspector;

namespace Conditions
{
    [Serializable]
    public class ItemCarried : SimplifiedCondition
    {
        [HorizontalGroup, HideLabel, SuffixLabel("x")]
        public required BaseItem Item;

        [HorizontalGroup, HideLabel]
        public uint AtLeastThisAmount = 1;

        protected override bool Filter(CharacterTemplate target, EvaluationContext context)
        {
            return target.Inventory.HasItem(Item, out uint count) && count >= AtLeastThisAmount;
        }

        public override bool IsValid([MaybeNullWhen(true)] out string error)
        {
            if (Item == null!)
            {
                error = $"{nameof(Item)} is null";
                return false;
            }

            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context) { }

        public override string UIDisplayText => $"has at least {AtLeastThisAmount} {Item?.name}";
    }
}