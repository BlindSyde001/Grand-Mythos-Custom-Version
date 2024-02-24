using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Interactables
{
    [Serializable]
    public class GiveItem : IInteraction
    {
        [Required]
        public CharacterTemplate.Drop Item;

        public IEnumerable<Delay> Interact(IInteractionSource source, OverworldPlayerControlsNode player)
        {
            InventoryManager.Instance.AddToInventory(Item.Item, Item.Count);
            yield break;
        }

        public bool IsValid(out string error)
        {
            if (Item.Item == null)
            {
                error = $"{nameof(Item)} is null";
                return false;
            }

            if (Item.Count == 0)
            {
                error = $"{nameof(Item.Count)} is 0";
                return false;
            }

            error = null;
            return true;
        }
    }
}