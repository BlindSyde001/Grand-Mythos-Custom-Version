using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Interactables;
using UnityEngine;

public interface IInteraction
{
    IEnumerable<Delay> InteractEnum(IInteractionSource source, OverworldPlayerController player);
    bool IsValid([MaybeNullWhen(true)] out string error);
}

public interface IInteractionSource
{
    public Transform transform { get; }
}

namespace Interactables
{
    public enum Delay
    {
        WaitTillNextFrame,
    }
}
