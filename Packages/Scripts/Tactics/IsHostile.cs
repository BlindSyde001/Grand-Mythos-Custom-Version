using System;

namespace Conditions
{
    [Serializable]
    public class IsHostile : SimplifiedCondition
    {
        public AliveState State = AliveState.Alive;

        protected override bool Filter(BattleCharacterController target, EvaluationContext context)
        {
            if (context.Controller.IsHostileTo(target))
            {
                return State switch
                {
                    AliveState.Alive => target.Profile.CurrentHP > 0,
                    AliveState.Dead => target.Profile.CurrentHP == 0,
                    AliveState.Either => true,
                    _ => throw new ArgumentOutOfRangeException(State.ToString())
                };
            }

            return false;
        }

        public override bool IsValid(out string error)
        {
            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context){}

        public override string UIDisplayText => State switch
        {
            AliveState.Alive => "is a living hostile",
            AliveState.Dead => "is a dead hostile",
            AliveState.Either => "is an hostile",
            _ => throw new ArgumentOutOfRangeException(State.ToString())
        };
    }

    public enum AliveState
    {
        Alive,
        Dead,
        Either,
    }
}