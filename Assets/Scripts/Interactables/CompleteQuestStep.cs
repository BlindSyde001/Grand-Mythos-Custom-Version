using System;
using System.Collections.Generic;
using Interactables;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class CompleteQuestStep : IInteraction
{
    [Required]
    public QuestStep Step;

    public IEnumerable<Delay> Interact(IInteractionSource source, OverworldPlayerController player)
    {
        if (GameManager.Instance.CompletedSteps.Add(Step) == false)
        {
            Debug.LogWarning($"Tried to complete step '{Step}' but it was already completed");
            yield break;
        }

        GameManager.Instance.DiscoveredQuests.Add(Step.Quest);
        if (Step.Quest.Steps[^1] == Step) // If this is the last step, run the outcome of this quest
        {
            foreach (var delay in Step.Quest.Outcome.Interact(source, player))
            {
                yield return delay;
            }
        }
    }

    public bool IsValid(out string error)
    {
        if (Step == null)
        {
            error = $"{nameof(Step)} is null";
            return false;
        }

        error = "";
        return true;
    }
}