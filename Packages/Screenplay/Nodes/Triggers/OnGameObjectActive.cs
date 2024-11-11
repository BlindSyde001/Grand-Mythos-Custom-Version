using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Screenplay.Nodes.Triggers
{
    [Serializable]
    public class OnGameObjectActive : ScreenplayNode, ITriggerSetup
    {
        [Required] public SceneObjectReference<GameObject> Target;

        public override void CollectReferences(List<GenericSceneObjectReference> references) { references.Add(Target); }

        public bool TryCreateTrigger(System.Action onTriggered, [MaybeNullWhen(false)] out ITrigger trigger)
        {
            if (Target.TryGet(out var go, out _) == false)
            {
                trigger = null;
                return false;
            }

            var comp = go.AddComponent<OnGameObjectActiveComp>();
            comp.Callback = onTriggered;
            trigger = comp;
            return true;
        }

        private class OnGameObjectActiveComp : MonoBehaviour, ITrigger
        {
            public System.Action Callback = null!;

            private void OnEnable()
            {
                Callback();
            }

            public void Dispose() => Destroy(this);
        }
    }
}
