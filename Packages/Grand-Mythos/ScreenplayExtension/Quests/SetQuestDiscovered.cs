using System;
using System.Collections.Generic;
using Screenplay;
using Screenplay.Nodes;
using Sirenix.OdinInspector;
using Action = Screenplay.Nodes.Action;

namespace Quests
{
    [Serializable]
    public class SetQuestDiscovered : Action
    {
        [Required, HideLabel] public Quest Quest;
        public bool Discovered = true;
        
        public override void CollectReferences(List<GenericSceneObjectReference> references)
        {
            
        }

        public override IEnumerable<Signal> Execute(IContext context)
        {
            Quest.Discovered = Discovered;
            yield return Signal.BreakInto(Next);
        }

        public override void FastForward(IContext context)
        {
            Quest.Discovered = Discovered;
        }

        public override void SetupPreview(IPreviewer previewer, bool fastForwarded)
        {
            var currentValue = Quest.Discovered;
            previewer.RegisterRollback(() => Quest.Discovered = currentValue);
            if (fastForwarded)
                FastForward(previewer);
            else
                previewer.PlaySafeAction(this);
        }
    }
}