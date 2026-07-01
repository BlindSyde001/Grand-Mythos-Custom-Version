using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Screenplay;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class Interaction : Precondition
{
    public required SceneObjectReference<GameObject> Target;

    public string Label = "Interact";

    public override void CollectReferences(ReferenceCollector references) => references.Collect(Target);

    public override async UniTask Setup(IPreconditionCollector tracker, Cancellation triggerCancellation)
    {
        while (triggerCancellation.IsCancellationRequested == false)
        {
            var target = await Target.GetAsync(triggerCancellation);

            var output = target.gameObject.AddComponent<InteractionComp>();
            try
            {
                output.Label = Label;
                while (output && triggerCancellation.IsCancellationRequested == false)
                {
                    await Uni.NextFrame(triggerCancellation, cancelImmediately: true); // This isn't great, but we must keep it open for at least one frame for all the other latches to open ...
                    tracker.SetUnlockedState(false);
                    await output.ResetEvent.NextSignal(triggerCancellation);
                    tracker.SetUnlockedState(true);
                }
            }
            finally
            {
                Object.Destroy(output);
            }
        }
    }
}

public class InteractionComp : MonoBehaviour
{
    /// <summary>
    /// Existing instances of this interactable,
    /// you can iterate over them to get their location,
    /// and trigger them if the player is close enough and pressed the interact button for example.
    /// </summary>
    public static IReadOnlyList<InteractionComp> Instances => s_instances;

    private static List<InteractionComp> s_instances = new();

    public required CancelableAutoResetEvent<bool> ResetEvent = new();
    public string Label = "?";

    private void OnEnable() => s_instances.Add(this);
    private void OnDisable() => s_instances.Remove(this);
    private void OnDestroy() => ResetEvent.Close();

    [Button("Force Trigger")]
    public void Trigger() => ResetEvent.Signal(false);
}