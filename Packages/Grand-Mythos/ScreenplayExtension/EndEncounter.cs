using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Screenplay;
using Screenplay.Nodes;
using UnityEngine;

[Serializable]
public class EndEncounter : ExecutableLinear
{
    /// <summary> Also skips battle rewards, needs additional work to account for that </summary>
    public bool SkipEndScreen = true;
    
    public override void CollectReferences(ReferenceCollector references) { }

    protected override async UniTask LinearExecution(IEventContext context, CancellationToken cancellation)
    {
        if (BattleStateMachine.TryGetInstance(out var battle))
        {
            await battle.ForceEnd(SkipEndScreen).WithInterruptingCancellation(cancellation);
        }
        else
        {
            Debug.LogError($"Could not {nameof(EndEncounter)} as no battles are currently running");
        }
    }

    public override UniTask Persistence(IEventContext context, CancellationToken cancellationToken)
    {
        return UniTask.CompletedTask;
    }

    public override void SetupPreview(IPreviewer previewer, bool fastForwarded) { }
}
