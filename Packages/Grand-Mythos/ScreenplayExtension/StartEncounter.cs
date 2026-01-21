using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Screenplay;
using Screenplay.Nodes;
using Sirenix.OdinInspector;
using UnityEngine;
using YNode;

[Serializable, NodeVisuals(Width = NodeVisualsAttribute.DefaultWidth  + 64)]
public class StartEncounter : AbstractScreenplayNode, IExecutable
{
    [Output, SerializeReference, HideLabel, Tooltip("What would run directly after the encounter starts")]
    public IExecutable? DuringEncounter;

    [Required, SerializeReference] public IEncounterDefinition Definition;

    public bool TestPrerequisite(HashSet<IPrerequisite> visited) => visited.Contains(this);

    public IEnumerable<IExecutable> Followup()
    {
        if (DuringEncounter != null)
            yield return DuringEncounter;
    }

    public override void CollectReferences(ReferenceCollector references)
    {
        
    }

    public async UniTask InnerExecution(IEventContext context, CancellationToken cancellation)
    {
        var encounterSignal = Definition.Start(OverworldPlayerController.Instances.First());
        while (encounterSignal.Signaled == false)
            await UniTask.NextFrame(cancellation, cancelImmediately: true);

        DuringEncounter?.Execute(context, cancellation);
    }

    public void FastForward(IEventContext context, CancellationToken cancellation)
    {
        
    }

    public void SetupPreview(IPreviewer previewer, bool fastForwarded)
    {
        // Let's not preview encounters, this is going to be quite complex if we don't want to break everything
    }
}