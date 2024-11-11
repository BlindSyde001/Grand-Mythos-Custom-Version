using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Sirenix.OdinInspector;
using UnityEngine;
using YNode;

namespace Screenplay.Nodes.Triggers
{
    [Serializable]
    public class MultiTrigger : ScreenplayNode, ITriggerSetup
    {
        [SerializeReference, Input, ListDrawerSettings(AlwaysAddDefaultValue = true, ShowFoldout = false)]
        public ITriggerSetup?[] Sources = Array.Empty<ITriggerSetup?>();

        public override void CollectReferences(List<GenericSceneObjectReference> references)
        {
            foreach (var source in Sources)
                source?.CollectReferences(references);
        }

        public bool TryCreateTrigger(System.Action onTriggered, [MaybeNullWhen(false)] out ITrigger trigger)
        {
            var list = new TriggerList();
            foreach (var setup in Sources)
            {
                if (setup is null)
                    continue;

                if (setup.TryCreateTrigger(onTriggered, out var otherTrigger) == false)
                {
                    list.Dispose();
                    trigger = null;
                    return false;
                }
                list.Triggers.Add(otherTrigger);
            }

            trigger = list;
            return true;
        }

        private class TriggerList : ITrigger
        {
            public List<ITrigger> Triggers = new();

            public void Dispose()
            {
                foreach (var trigger in Triggers)
                    trigger.Dispose();
            }
        }
    }
}
