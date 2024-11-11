using System.Collections.Generic;
using UnityEngine;

namespace Screenplay.Nodes
{
    public class Wait : Action
    {
        public float Duration = 1f;

        public override void CollectReferences(List<GenericSceneObjectReference> references) { }

        public override IEnumerable<Signal> Execute(IContext context)
        {
            for (float f = 0; f < Duration; f += Time.deltaTime)
            {
                yield return Signal.NextFrame;
            }

            yield return Signal.BreakInto(Next);
        }

        public override void FastForward(IContext context) { }

        public override void SetupPreview(IPreviewer previewer, bool fastForwarded) { }
    }
}
