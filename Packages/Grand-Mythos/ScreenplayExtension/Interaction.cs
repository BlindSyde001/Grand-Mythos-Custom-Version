using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Screenplay;
using Screenplay.Nodes;
using Screenplay.Nodes.Triggers;
using Sirenix.OdinInspector;
using UnityEngine;


[Serializable]
public class Interaction : ScreenplayNode, ITriggerSetup
{
    [Required] public SceneObjectReference<GameObject> Target;

    public string Label = "Interact";

    public override void CollectReferences(List<GenericSceneObjectReference> references) => references.Add(Target);

    public bool TryCreateTrigger(System.Action onTriggered, [MaybeNullWhen(false)] out ITrigger trigger)
    {
        if (Target.TryGet(out var obj, out _) == false)
        {
            trigger = null;
            return false;
        }

        var output = obj.AddComponent<InteractionComp>();
        output.Callback = onTriggered;
        output.Label = Label;
        trigger = output;
        return true;
    }
}

public class InteractionComp : MonoBehaviour, ITrigger
{
    /// <summary>
    /// Existing instances of this interactable,
    /// you can iterate over them to get their location,
    /// and trigger them if the player is close enough and pressed the interact button for example.
    /// </summary>
    public static IReadOnlyList<InteractionComp> Instances => s_instances;

    private static List<InteractionComp> s_instances = new();

    public string Label = "?";

    /// <summary>
    /// This callback will start the associated event when invoked
    /// </summary>
    public System.Action Callback = null!;

    private void OnEnable() => s_instances.Add(this);
    private void OnDisable() => s_instances.Remove(this);

    [Button("Force Trigger")]
    public void Trigger() => Callback.Invoke();

    public void Dispose() => Destroy(this);
}
