using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Interactables
{
    /// <summary>
    /// This class is very much a work in progress, dialog content should be stored inside of a serialized object to be localized.
    /// Likely need dialog choices as well, and a better UI handler than using the prompt one.
    /// </summary>
    [Serializable]
    public class TalkToNPC : IInteraction
    {
        public string DialogContent = "This is a very basic implementation for NPC dialog";
        [Required] public Transform DialogAnchor;

        public IEnumerable<Delay> Interact(IInteractionSource source, OverworldPlayerController player)
        {
            for (float time = 0; time < 2f; time += Time.deltaTime)
            {
                Prompt.TryShowPromptThisFrame(DialogAnchor.transform.position, DialogContent);
                yield return Delay.WaitTillNextFrame;
            }
        }

        public bool IsValid(out string error)
        {
            if (DialogAnchor == null)
            {
                error = $"{nameof(DialogAnchor)} is null";
                return false;
            }

            error = null;
            return true;
        }
    }
}