using System;
using Sirenix.OdinInspector;

namespace Conditions
{
    [Serializable]
    public class ItemCarried : SimplifiedCondition
    {
        [Required, HorizontalGroup, HideLabel, SuffixLabel("x")]
        public BaseItem Item;

        [HorizontalGroup, HideLabel]
        public uint AtLeastThisAmount = 1;

        protected override bool Filter(CharacterTemplate target, EvaluationContext context)
        {
            return target.Inventory.HasItem(Item, out uint count) && count <= AtLeastThisAmount;
        }

        public override bool IsValid(out string error)
        {
            if (Item == null)
            {
                error = $"{nameof(Item)} is null";
                return false;
            }

            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context) { }

        public override string UIDisplayText => $"has at least {AtLeastThisAmount} {Item}";
    }
}