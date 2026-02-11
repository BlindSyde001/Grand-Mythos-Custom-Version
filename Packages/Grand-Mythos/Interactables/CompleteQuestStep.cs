using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Interactables;
using UnityEngine;

[Serializable]
public class CompleteQuestStep : IInteraction
{
    public required QuestStep Step;

    public UniTask InteractEnum(IInteractionSource source, OverworldPlayerController player)
    {
        if (Step.Completed)
        {
            Debug.LogWarning($"Tried to complete step '{Step}' but it was already completed");
            return UniTask.CompletedTask;
        }

        Step.Quest.Discovered = true;
        Step.Completed = true;
        if (Step.Quest.Completed && Step.Quest.Outcome is not null) // If this was the last step to complete this quest, run the outcome of this quest
        {
            return Step.Quest.Outcome.InteractEnum(source, player);
        }

        return UniTask.CompletedTask;
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

    public void DuringSceneGui(IInteractionSource source, SceneGUIProxy sceneGUI) { }
}