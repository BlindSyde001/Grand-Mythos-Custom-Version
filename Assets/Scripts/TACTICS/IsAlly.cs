using System;

namespace Conditions
{
    [Serializable]
    public class IsAlly : SimplifiedCondition
    {
        public AliveState State = AliveState.Alive;

        protected override bool Filter(CharacterTemplate target, EvaluationContext context)
        {
            if (context.Source.IsHostileTo(target) == false)
            {
                return State switch
                {
                    AliveState.Alive => target._CurrentHP > 0,
                    AliveState.Dead => target._CurrentHP == 0,
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
            AliveState.Alive => "is a living ally",
            AliveState.Dead => "is a dead ally",
            AliveState.Either => "is an ally",
            _ => throw new ArgumentOutOfRangeException(State.ToString())
        };
    }
}