using System;
using System.Collections.Generic;
using Nodalog;
using Sirenix.OdinInspector;

namespace Interactables
{
    [Serializable]
    public class SetFlag : IInteraction
    {
        [Required, HorizontalGroup, HideLabel]
        public Flag Flag;

        [Required, HorizontalGroup, LabelText(" \u2192 ")]
        public bool NewState;

        public IEnumerable<Delay> InteractEnum(IInteractionSource source, OverworldPlayerController player)
        {
            Flag.State = NewState;
            yield break;
        }

        public bool IsValid(out string error)
        {
            error = null;
            return true;
        }
    }
}