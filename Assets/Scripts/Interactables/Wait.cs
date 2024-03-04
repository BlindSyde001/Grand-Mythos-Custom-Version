using System;
using System.Collections.Generic;
using UnityEngine;

namespace Interactables
{
    [Serializable]
    public class Wait : IInteraction
    {
        public float DurationInSeconds = 1f;
        public IEnumerable<Delay> Interact(IInteractionSource source, OverworldPlayerController player)
        {
            for (float f = 0; f < DurationInSeconds; f += Time.deltaTime)
            {
                yield return Delay.WaitTillNextFrame;
            }
        }

        public bool IsValid(out string error)
        {
            if (DurationInSeconds < 0)
            {
                error = $"{nameof(DurationInSeconds)} is negative ({DurationInSeconds})";
                return false;
            }

            error = null;
            return true;
        }
    }
}