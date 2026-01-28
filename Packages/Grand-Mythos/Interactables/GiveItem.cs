using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Sirenix.OdinInspector;

namespace Interactables
{
    [Serializable]
    public class GiveItem : IInteraction
    {
        public required BaseItem Item;
        public required uint Count = 1;

        bool ValidateCount(uint count) => count > 0;

        public IEnumerable<Delay> InteractEnum(IInteractionSource source, OverworldPlayerController player)
        {
            InventoryManager.Instance.AddToInventory(Item, Count);
            yield break;
        }

        public bool IsValid([MaybeNullWhen(true)] out string error)
        {
            if (Item == null!)
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