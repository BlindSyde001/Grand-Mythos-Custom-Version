using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Screenplay;
using Screenplay.Nodes;
using UnityEngine;

[Serializable]
public class ContinueEncounter : ExecutableLinear
{
    protected override async UniTask LinearExecution(IEventContext context, CancellationToken cancellation)
    {
        if (BattleStateMachine.TryGetInstance(out var battle))
        {
            await battle.Unpause(cancellation);
        }
        else
        {
            Debug.LogError($"Could not {nameof(AwaitEndOfEncounter)} as no battles are currently running");
        }

    }

    public override UniTask Persistence(IEventContext context, CancellationToken cancellationToken) => UniTask.CompletedTask;

    public override void SetupPreview(IPreviewer previewer, bool fastForwarded) { }

    public override void CollectReferences(ReferenceCollector references){}
}
