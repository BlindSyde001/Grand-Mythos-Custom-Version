using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Events;

namespace Interactables
{
    [Serializable]
    public class RunEvent : IInteraction
    {
        public UnityEvent? Event;

        public IEnumerable<Delay> InteractEnum(IInteractionSource source, OverworldPlayerController player)
        {
            Event?.Invoke();
            yield break;
        }

        public bool IsValid([MaybeNullWhen(true)] out string error)
        {
            error = null;
            return true;
        }
    }
}