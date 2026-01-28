using System;
using System.Collections.Generic;
using Interactables;

[Serializable]
public class DiscoverQuest : IInteraction
{
    [Tooltip("This quest will be added to the journal")]
    public required Quest Quest;

    public IEnumerable<Delay> InteractEnum(IInteractionSource source, OverworldPlayerController player)
    {
        Quest.Discovered = true;
        yield break;
    }

    public bool IsValid(out string error)
    {
        if (Quest == null!)
        {
            error = $"{nameof(Quest)} is null";
            return false;
        }

        error = "";
        return true;
    }
}