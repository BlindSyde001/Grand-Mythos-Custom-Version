using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nodalog;
using Sirenix.OdinInspector;

namespace Interactables
{
    [Serializable]
    public class SetFlag : IInteraction
    {
        [HorizontalGroup, HideLabel]
        public required Flag Flag;

        [HorizontalGroup, LabelText(" \u2192 ")]
        public bool NewState;

        public IEnumerable<Delay> InteractEnum(IInteractionSource source, OverworldPlayerController player)
        {
            Flag.State = NewState;
            yield break;
        }

        public bool IsValid([MaybeNullWhen(true)] out string error)
        {
            error = null;
            return true;
        }
    }
}