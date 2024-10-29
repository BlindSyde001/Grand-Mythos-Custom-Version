using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Interactables
{
    [Serializable]
    public class TryRemoveItem : IInteraction
    {
        [Required]
        public BaseItem Item;
        [ValidateInput(nameof(ValidateCount), "Must be greater than 0!")]
        public uint Count = 1;

        bool ValidateCount(uint count) => count > 0;

        public IEnumerable<Delay> InteractEnum(IInteractionSource source, OverworldPlayerController player)
        {
            if (InventoryManager.Instance.FindItem(Item, out var existingCount))
            {
                if (existingCount >= Count)
                {
                    InventoryManager.Instance.Remove(Item, Count);
                }
                else
                {
                    Debug.LogWarning($"Could not remove {Count} {Item}, there are only {existingCount} of this item, removing {existingCount} instead");
                    InventoryManager.Instance.Remove(Item, existingCount);
                }
            }
            else
            {
                Debug.LogWarning($"Could not find any {Item}, aborting remove");
            }

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