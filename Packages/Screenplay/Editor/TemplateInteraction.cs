#if _
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Sirenix.OdinInspector;
using UnityEngine;
using Screenplay;
using Screenplay.Nodes;
using Screenplay.Nodes.Triggers;

[Serializable]
public class __SCRIPTNAME__ : ScreenplayNode, ITriggerSetup
{
    // This class is not the object that is interactable, it specifically handles setting up interactable objects in the scene, see below

    // The scene object this interaction will be attached to
    [Required] public SceneObjectReference<GameObject> Target;

    // Append any reference you have above in this function to ensure they work as expected
    public override void CollectReferences(List<GenericSceneObjectReference> references) => references.Add(Target);

    // This is called by the screenplay when the progression of the story reached the event this interaction is attached to
    public bool TryCreateTrigger(System.Action onTriggered, [MaybeNullWhen(false)] out ITrigger trigger)
    {
        // Here we check whether the target is part of the existing scene
        if (Target.TryGet(out var obj, out _) == false)
        {
            // If the target does not exist, we'll let the screenplay know that this event is not reachable yet,
            // Screenplay will try again later in that case
            trigger = null;
            return false;
        }

        // Since the target exists, we'll add the interactable object so that the player can interact with it and trigger the associated event
        var output = obj.AddComponent<__SCRIPTNAME__Comp>();
        output.Callback = onTriggered; // This onTriggered is the callback for the screenplay to start the event, you should invoke it whenever the player interacts with the component
        trigger = output;
        return true;
    }

    // This is the actual object which hosts the logic to handle the interaction itself
    public class __SCRIPTNAME__Comp : MonoBehaviour, ITrigger
    {
        /// <summary>
        /// Existing instances of this interactable,
        /// you can iterate over them to get their location,
        /// and trigger them if the player is close enough and pressed the interact button for example.
        /// </summary>
        public static IReadOnlyList<__SCRIPTNAME__Comp> Instances => s_instances;

        private static List<__SCRIPTNAME__Comp> s_instances = new();

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
}
#endif // _
