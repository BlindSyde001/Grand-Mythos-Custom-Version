using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Interactables;

public class TeleportToSpawn : IInteraction
{
    public required SpawnPointReference Target;

    public UniTask InteractEnum(IInteractionSource source, OverworldPlayerController player)
    {
        Target.SwapSceneToThisSpawnPoint();
        return UniTask.CompletedTask;
    }

    public bool IsValid(out string error)
    {
        if (Target == null!)
        {
            error = "No spawn point assigned";
            return false;
        }

        error = "";
        return true;
    }

    public void DuringSceneGui(IInteractionSource source, SceneGUIProxy sceneGUI) { }
}