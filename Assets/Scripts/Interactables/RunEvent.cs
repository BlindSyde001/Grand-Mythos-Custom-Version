using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Interactables
{
    [Serializable]
    public class RunEvent : IInteraction
    {
        public UnityEvent Event;

        public IEnumerable<Delay> Interact(IInteractionSource source, OverworldPlayerControlsNode player)
        {
            Event?.Invoke();
            yield break;
        }

        public bool IsValid(out string error)
        {
            error = null;
            return true;
        }
    }
}