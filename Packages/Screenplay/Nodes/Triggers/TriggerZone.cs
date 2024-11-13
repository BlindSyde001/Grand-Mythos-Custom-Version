using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Screenplay.Nodes.Triggers
{
    [Serializable]
    public class TriggerZone : ScreenplayNode, ITriggerSetup
    {
        [Required, ValidateInput(nameof(IsTrigger))] public SceneObjectReference<Collider> Target;
        public LayerMask LayerMask = ~0;

        private bool IsTrigger(SceneObjectReference<Collider> target, ref string message)
        {
            if (target.TryGet(out var obj, out _) && obj.isTrigger == false)
            {
                message = $"{obj.name} must have {nameof(obj.isTrigger)} enabled";
                return false;
            }

            return true;
        }

        public override void CollectReferences(List<GenericSceneObjectReference> references) => references.Add(Target);

        public bool TryCreateTrigger(System.Action onTriggered, [MaybeNullWhen(false)] out ITrigger trigger)
        {
            if (Target.TryGet(out var obj, out _) == false)
            {
                trigger = null;
                return false;
            }

            var output = obj.gameObject.AddComponent<TriggerZoneComponent>();
            output.Callback = onTriggered;
            output.LayerMask = LayerMask;
            trigger = output;
            return true;
        }

        private class TriggerZoneComponent : MonoBehaviour, ITrigger
        {
            public System.Action Callback = null!;
            public LayerMask LayerMask;

            private void OnTriggerStay(Collider collider)
            {
                GameObject go;
                if (collider is CharacterController cc)
                    go = cc.gameObject;
                else
                    go = collider.attachedRigidbody.gameObject;

                if ((LayerMask & 1 << go.layer) != 0)
                    Trigger();
            }

            [Button("Force Trigger")]
            public void Trigger() => Callback.Invoke();

            public void Dispose() => Destroy(this);
        }
    }
}
