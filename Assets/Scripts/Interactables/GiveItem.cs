using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Interactables
{
    [Serializable]
    public class GiveItem : IInteraction
    {
        [Required]
        public ItemCapsule Item;

        public IEnumerable<Delay> Interact(IInteractionSource source, OverworldPlayerControlsNode player)
        {
            InventoryManager._instance.AddToInventory(Item);
            yield break;
        }

        public bool IsValid(out string error)
        {
            if (Item == null || Item.thisItem == null)
            {
                error = $"{nameof(Item)} is null";
                return false;
            }

            if (Item.ItemAmount == 0)
            {
                error = $"{nameof(Item.ItemAmount)} is 0";
                return false;
            }

            error = null;
            return true;
        }
    }
}