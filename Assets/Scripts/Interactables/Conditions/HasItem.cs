using System;
using Sirenix.OdinInspector;

namespace Interactables.Conditions
{
    [Serializable]
    public class HasItem : ICondition
    {
        [Required]
        public BaseItem Item;
        [ValidateInput(nameof(ValidateCount), "Must be greater than 0!")]
        public uint Count = 1;

        public bool Evaluate() => InventoryManager.Instance.FindItem(Item, out var count) && count >= Count;

        public bool IsValid(out string error)
        {
            if (Item == null)
            {
                error = "Item is null";
                return false;
            }

            if (Count == 0)
            {
                error = "Count must be greater than 0";
                return false;
            }

            error = "";
            return true;
        }

        bool ValidateCount(uint count) => count > 0;
    }
}