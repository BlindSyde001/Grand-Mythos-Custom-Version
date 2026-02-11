using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Interactables
{
    [Serializable]
    public class TryRemoveItem : IInteraction
    {
        public required BaseItem Item;
        public required uint Count = 1;

        bool ValidateCount(uint count) => count > 0;

        public UniTask InteractEnum(IInteractionSource source, OverworldPlayerController player)
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

            return UniTask.CompletedTask;
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

        public void DuringSceneGui(IInteractionSource source, SceneGUIProxy sceneGUI) { }
    }
}