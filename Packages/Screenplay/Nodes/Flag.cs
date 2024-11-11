using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Screenplay.Nodes
{
    public class Flag : Action
    {
        [HideLabel]
        public string Description = "Description";

        public override void CollectReferences(List<GenericSceneObjectReference> references){ }

        public override IEnumerable<Signal> Execute(IContext context)
        {
            yield return Signal.BreakInto(Next);
        }

        public override void FastForward(IContext context) { }

        public override void SetupPreview(IPreviewer previewer, bool fastForwarded) { }
    }
}
