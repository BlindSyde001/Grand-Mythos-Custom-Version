using System;
using System.Collections.Generic;
using Screenplay;
using Screenplay.Nodes;
using Sirenix.OdinInspector;
using Action = Screenplay.Nodes.Action;

namespace Quests
{
    [Serializable]
    public class SetQuestStep : Action
    {
        [Required, HideLabel] public QuestStep Step;
        public bool Completed = true;
        
        public override void CollectReferences(List<GenericSceneObjectReference> references)
        {
            
        }

        public override IEnumerable<Signal> Execute(IContext context)
        {
            Step.Completed = Completed;
            yield return Signal.BreakInto(Next);
        }

        public override void FastForward(IContext context)
        {
            Step.Completed = Completed;
        }

        public override void SetupPreview(IPreviewer previewer, bool fastForwarded)
        {
            var currentValue = Step.Completed;
            previewer.RegisterRollback(() => Step.Completed = currentValue);
            if (fastForwarded)
                FastForward(previewer);
            else
                previewer.PlaySafeAction(this);
        }
    }
}