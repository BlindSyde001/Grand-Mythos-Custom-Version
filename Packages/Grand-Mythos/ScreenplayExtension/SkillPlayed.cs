using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Screenplay;
using Sirenix.OdinInspector;

namespace Quests
{
    [Serializable]
    public class SkillPlayed : Precondition
    {
        [HideLabel] public required Skill Skill;
        public Unit Filter = Unit.Any;
        
        public override void CollectReferences(ReferenceCollector references) { }
        public override async UniTask Setup(IPreconditionCollector tracker, CancellationToken triggerCancellation)
        {
            while (triggerCancellation.IsCancellationRequested == false)
            {
                tracker.SetUnlockedState(Evaluate());
                await UniTask.NextFrame(triggerCancellation, cancelImmediately: true);
            }
        }

        private bool Evaluate()
        {
            if (BattleStateMachine.TryGetInstance(out var battleStateMachine) == false
                || battleStateMachine.TacticsPlaying is null)
                return false;

            if (battleStateMachine.TacticsPlaying.Action is Skill s == false)
                return false;
            if (s != Skill)
                return false;

            return Filter switch
            {
                Unit.Any => true,
                Unit.Hostile => battleStateMachine.PartyLineup.Contains(battleStateMachine.UnitPlaying!) == false,
                Unit.Ally => battleStateMachine.PartyLineup.Contains(battleStateMachine.UnitPlaying!),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public enum Unit
        {
            Any,
            Hostile,
            Ally
        }
    }
}