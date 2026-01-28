using System.Collections.Generic;
using Interactables;

public class TeleportToSpawn : IInteraction
{
    public required SpawnPointReference Target;

    public IEnumerable<Delay> InteractEnum(IInteractionSource source, OverworldPlayerController player)
    {
        Target.SwapSceneToThisSpawnPoint();
        yield break;
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
}