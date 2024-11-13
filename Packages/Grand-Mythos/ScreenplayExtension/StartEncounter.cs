using System;
using System.Collections.Generic;
using System.Linq;
using Screenplay;
using Screenplay.Nodes;
using Sirenix.OdinInspector;
using UnityEngine;
using YNode;

[Serializable, NodeWidth(NodeWidthAttribute.Default + 64)]
public class StartEncounter : ScreenplayNode, Screenplay.Nodes.IAction
{
    [Output, SerializeReference, HideLabel, Tooltip("What would run directly after the encounter starts")]
    public Screenplay.Nodes.IAction? DuringEncounter;

    [Required, SerializeReference] public IEncounterDefinition Definition;

    public bool TestPrerequisite(HashSet<IPrerequisite> visited) => visited.Contains(this);

    public IEnumerable<Screenplay.Nodes.IAction> Followup()
    {
        if (DuringEncounter != null)
            yield return DuringEncounter;
    }
        
    public override void CollectReferences(List<GenericSceneObjectReference> references)
    {
        
    }

    public IEnumerable<Signal> Execute(IContext context)
    {
        var encounterSignal = Definition.Start(OverworldPlayerController.Instances.First());
        while (encounterSignal.Signaled == false)
            yield return Signal.NextFrame;

        yield return Signal.BreakInto(DuringEncounter);
    }

    public void FastForward(IContext context)
    {
        
    }

    public void SetupPreview(IPreviewer previewer, bool fastForwarded)
    {
        // Let's not preview encounters, this is going to be quite complex if we don't want to break everything
    }
}