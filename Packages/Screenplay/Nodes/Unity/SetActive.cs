using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Screenplay.Nodes.Unity
{
    public class SetActive : Action
    {
        [Required, HideLabel, HorizontalGroup] public SceneObjectReference<GameObject> Target;
        [HideLabel, HorizontalGroup(width:16)] public bool Active = true;

        public override void CollectReferences(List<GenericSceneObjectReference> references) => references.Add(Target);

        public override IEnumerable<Signal> Execute(IContext context)
        {
            if (Target.TryGet(out var target, out _))
                target.SetActive(Active);
            yield return Signal.BreakInto(Next);
        }

        public override void FastForward(IContext context)
        {
            if (Target.TryGet(out var target, out _))
                target.SetActive(Active);
        }

        public override void SetupPreview(IPreviewer previewer, bool fastForwarded)
        {
            if (Target.TryGet(out var target, out _))
            {
                var currentValue = target.activeSelf;
                previewer.RegisterRollback(() => target.SetActive(currentValue));
                if (fastForwarded)
                    FastForward(previewer);
                else
                    previewer.PlaySafeAction(this);
            }
        }
    }
}
