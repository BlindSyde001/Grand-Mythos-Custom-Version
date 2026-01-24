using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Screenplay;
using Screenplay.Nodes;
using UnityEngine;
using YNode;

[Serializable]
public class AwaitEndOfEncounter : AbstractScreenplayNode, IExecutable
{
    [Output, SerializeReference, Tooltip("What would run directly after the encounter starts")]
    public ExecutableLinear? Victory, Defeat;

    public bool TestPrerequisite(HashSet<IPrerequisite> visited) => visited.Contains(this);

    public IEnumerable<IExecutable> Followup()
    {
        if (Victory != null)
            yield return Victory;
        
        if (Defeat != null)
            yield return Defeat;
    }

    public async UniTask<IExecutable?> InnerExecution(IEventContext context, CancellationToken cancellation)
    {
        if (BattleStateMachine.TryGetInstance(out var battle))
        {
            while (battle.Finished.IsCompleted == false)
                await UniTask.NextFrame(cancellation, cancelImmediately: true);

            return battle.Finished.Result ? Victory : Defeat;
        }

        Debug.LogError($"Could not {nameof(AwaitEndOfEncounter)} as no battles are currently running, defaulting to 'victory' outcome");
        return Victory;
    }

    public UniTask Persistence(IEventContext context, CancellationToken cancellation) => UniTask.CompletedTask;

    public override void CollectReferences(ReferenceCollector references){}

    public void SetupPreview(IPreviewer previewer, bool fastForwarded)
    {
        // Let's not preview encounters, this is going to be quite complex if we don't want to break everything
    }
}