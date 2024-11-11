using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using YNode;

namespace Screenplay.Nodes
{
    public class Event : ScreenplayNode
    {
        [HideInInspector, SerializeField]
        public string Name = "My Unnamed Event";

        [Output, SerializeReference, Tooltip("What would be running when this event starts"), Required]
        public IAction? Action;
        [Tooltip("Can this event ever run again after having been completed")]
        public bool Repeatable;
        [Input(Stroke = NoodleStroke.Dashed), SerializeReference, Tooltip("Which nodes need to be visited for this event to become executable")]
        public IPrerequisite? Prerequisite;
        [Input, SerializeReference, Tooltip("Interaction setup for the sole purpose of triggering this event")]
        public ITriggerSetup? TriggerSource;

        public override void CollectReferences(List<GenericSceneObjectReference> references) { }
    }
}
