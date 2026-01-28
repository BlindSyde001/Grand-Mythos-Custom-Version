using System;
using System.Diagnostics.CodeAnalysis;

namespace Conditions
{
    [Serializable]
    public class IsAlly : SimplifiedCondition
    {
        public AliveState State = AliveState.Alive;

        protected override bool Filter(BattleCharacterController target, EvaluationContext context)
        {
            if (context.Controller.IsHostileTo(target) == false)
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

        public override bool IsValid([MaybeNullWhen(true)] out string error)
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