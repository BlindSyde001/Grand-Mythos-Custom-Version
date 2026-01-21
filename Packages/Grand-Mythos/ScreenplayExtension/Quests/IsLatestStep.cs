using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Screenplay;
using Sirenix.OdinInspector;

namespace Quests
{
    public class IsCurrentStep : Precondition
    {
        [Required, HideLabel] public QuestStep Step;
        
        public override void CollectReferences(ReferenceCollector references) { }

        public override async UniTask Setup(IPreconditionCollector tracker, CancellationToken triggerCancellation)
        {
            while (triggerCancellation.IsCancellationRequested == false)
            {
                tracker.SetUnlockedState(TestPrerequisite());
                await UniTask.NextFrame(triggerCancellation, cancelImmediately: true);
            }
        }

        private bool TestPrerequisite()
        {
            if (Step.Quest.Discovered == false)
                return false;
            
            if (Step.Completed)
                return false;

            var i = Array.IndexOf(Step.Quest.Steps, Step);
            return i == 0 || Step.Quest.Steps[i-1].Completed;
        }
    }
}