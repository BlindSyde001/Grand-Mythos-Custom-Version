using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Screenplay;
using Screenplay.Nodes;
using Sirenix.OdinInspector;

namespace Quests
{
    [Serializable]
    public class SetQuestStep : ExecutableLinear
    {
        [Required, HideLabel] public QuestStep Step;
        public bool Completed = true;
        
        public override void CollectReferences(ReferenceCollector references)
        {
            
        }

        protected override UniTask LinearExecution(IEventContext context, CancellationToken cancellation)
        {
            Step.Completed = Completed;
            return UniTask.CompletedTask;
        }

        public override UniTask Persistence(IEventContext context, CancellationToken cancellationToken)
        {
            return LinearExecution(context, cancellationToken);
        }

        public override void SetupPreview(IPreviewer previewer, bool fastForwarded)
        {
            var currentValue = Step.Completed;
            previewer.RegisterRollback(() => Step.Completed = currentValue);
            if (fastForwarded)
                Persistence(previewer, CancellationToken.None);
            else
                previewer.PlaySafeAction(this);
        }
    }
}