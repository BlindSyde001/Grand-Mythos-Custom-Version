using System.Collections.Generic;
using Interactables;
using Sirenix.OdinInspector;

public class TeleportToSpawn : IInteraction
{
    [Required] public SpawnPointReference Target;

    public IEnumerable<Delay> Interact(IInteractionSource source, OverworldPlayerController player)
    {
        Target.SwapSceneToThisSpawnPoint();
        yield break;
    }

    public bool IsValid(out string error)
    {
        if (Target == null)
        {
            error = "No spawn point assigned";
            return false;
        }

        error = "";
        return true;
    }
}