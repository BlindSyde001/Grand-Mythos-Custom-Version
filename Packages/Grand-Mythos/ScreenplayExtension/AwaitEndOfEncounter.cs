using System;
using System.Collections.Generic;
using Screenplay;
using Screenplay.Nodes;
using UnityEngine;
using YNode;

[Serializable]
public class AwaitEndOfEncounter : ScreenplayNode, Screenplay.Nodes.IAction
{
    [Output, SerializeReference, Tooltip("What would run directly after the encounter starts")]
    public Screenplay.Nodes.IAction? Victory, Defeat;

    public bool TestPrerequisite(HashSet<IPrerequisite> visited) => visited.Contains(this);

    public IEnumerable<Screenplay.Nodes.IAction> Followup()
    {
        if (Victory != null)
            yield return Victory;
        
        if (Defeat != null)
            yield return Defeat;
    }
        
    public override void CollectReferences(List<GenericSceneObjectReference> references)
    {
        
    }

    public IEnumerable<Signal> Execute(IContext context)
    {
        if (BattleStateMachine.TryGetInstance(out var battle))
        {
            while (battle.Finished.IsCompleted == false)
                yield return Signal.NextFrame;

            yield return Signal.BreakInto(battle.Finished.Result ? Victory : Defeat);
        }

        Debug.LogError($"Could not {nameof(AwaitEndOfEncounter)} as no battles are currently running, defaulting to 'victory' outcome");
        yield return Signal.BreakInto(Victory);
    }

    public void FastForward(IContext context)
    {
        
    }

    public void SetupPreview(IPreviewer previewer, bool fastForwarded)
    {
        // Let's not preview encounters, this is going to be quite complex if we don't want to break everything
    }
}