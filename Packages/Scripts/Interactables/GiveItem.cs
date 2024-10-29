using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Interactables
{
    [Serializable]
    public class GiveItem : IInteraction
    {
        [Required]
        public BaseItem Item;
        [ValidateInput(nameof(ValidateCount), "Must be greater than 0!")]
        public uint Count = 1;

        bool ValidateCount(uint count) => count > 0;

        public IEnumerable<Delay> InteractEnum(IInteractionSource source, OverworldPlayerController player)
        {
            InventoryManager.Instance.AddToInventory(Item, Count);
            yield break;
        }

        public bool IsValid(out string error)
        {
            if (Item == null)
            {
                error = $"{nameof(Item)} is null";
                return false;
            }

            if (Count == 0)
            {
                error = $"{nameof(Count)} is 0";
                return false;
            }

            error = null;
            return true;
        }
    }
}