using System;
using System.Diagnostics.CodeAnalysis;

namespace Conditions
{
    [Serializable]
    public class IsHostile : SimplifiedCondition
    {
        public AliveState State = AliveState.Alive;

        protected override bool Filter(CharacterTemplate target, EvaluationContext context)
        {
            if (context.Profile.IsHostileTo(target))
            {
                return State switch
                {
                    AliveState.Alive => target.CurrentHP > 0,
                    AliveState.Dead => target.CurrentHP == 0,
                    AliveState.Either => true,
                    _ => throw new ArgumentOutOfRangeException(State.ToString())
                };
            }

            return false;
        }

        public override bool IsValid([MaybeNullWhen(true)] out string error)
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