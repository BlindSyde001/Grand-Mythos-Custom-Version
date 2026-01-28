using System;
using System.Collections.Generic;
using Interactables;
using UnityEngine;

[Serializable]
public class CompleteQuestStep : IInteraction
{
    public required QuestStep Step;

    public IEnumerable<Delay> InteractEnum(IInteractionSource source, OverworldPlayerController player)
    {
        if (Step.Completed)
        {
            Debug.LogWarning($"Tried to complete step '{Step}' but it was already completed");
            yield break;
        }

        Step.Quest.Discovered = true;
        Step.Completed = true;
        if (Step.Quest.Completed && Step.Quest.Outcome is not null) // If this was the last step to complete this quest, run the outcome of this quest
        {
            foreach (var delay in Step.Quest.Outcome.InteractEnum(source, player))
            {
                yield return delay;
            }
        }
    }

    public bool IsValid(out string error)
    {
        if (Step == null!)
        {
            error = $"{nameof(Step)} is null";
            return false;
        }

        error = "";
        return true;
    }
}