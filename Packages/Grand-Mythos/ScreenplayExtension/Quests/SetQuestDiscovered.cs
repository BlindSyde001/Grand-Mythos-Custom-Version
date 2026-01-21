using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Screenplay;
using Screenplay.Nodes;
using Sirenix.OdinInspector;

namespace Quests
{
    [Serializable]
    public class SetQuestDiscovered : ExecutableLinear
    {
        [Required, HideLabel] public Quest Quest;
        public bool Discovered = true;
        
        public override void CollectReferences(ReferenceCollector references)
        {
            
        }

        protected override UniTask LinearExecution(IEventContext context, CancellationToken cancellation)
        {
            Quest.Discovered = Discovered;
            return UniTask.CompletedTask;
        }

        public override void FastForward(IEventContext context, CancellationToken cancellationToken)
        {
            Quest.Discovered = Discovered;
        }

        public override void SetupPreview(IPreviewer previewer, bool fastForwarded)
        {
            var currentValue = Quest.Discovered;
            previewer.RegisterRollback(() => Quest.Discovered = currentValue);
            if (fastForwarded)
                FastForward(previewer, CancellationToken.None);
            else
                previewer.PlaySafeAction(this);
        }
    }
}