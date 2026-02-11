using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Interactables;

[Serializable]
public class DiscoverQuest : IInteraction
{
    [Tooltip("This quest will be added to the journal")]
    public required Quest Quest;

    public UniTask InteractEnum(IInteractionSource source, OverworldPlayerController player)
    {
        Quest.Discovered = true;
        return UniTask.CompletedTask;
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

    public void DuringSceneGui(IInteractionSource source, SceneGUIProxy sceneGUI) { }
}