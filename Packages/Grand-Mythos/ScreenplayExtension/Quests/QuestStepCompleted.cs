using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Screenplay;
using Sirenix.OdinInspector;

namespace Quests
{
    [Serializable]
    public class QuestStepCompleted : Precondition
    {
        [Required, HideLabel] public QuestStep Step;
        
        public override void CollectReferences(ReferenceCollector references) { }
        public override async UniTask Setup(IPreconditionCollector tracker, CancellationToken triggerCancellation)
        {
            while (triggerCancellation.IsCancellationRequested == false)
            {
                tracker.SetUnlockedState(Step.Completed);
                await UniTask.NextFrame(triggerCancellation, cancelImmediately: true);
            }
        }
    }
}