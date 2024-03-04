using System.Collections.Generic;
using Interactables;
using UnityEngine;

public interface IInteraction
{
    IEnumerable<Delay> Interact(IInteractionSource source, OverworldPlayerController player);
    bool IsValid(out string error);
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
