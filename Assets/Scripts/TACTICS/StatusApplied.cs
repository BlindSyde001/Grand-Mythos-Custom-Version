﻿using System;

namespace Conditions
{
    [Serializable]
    public class StatusApplied : SimplifiedCondition
    {
        public Status Status;

        protected override bool Filter(CharacterTemplate target, EvaluationContext context) => target.HasStatus(Status);

        public override bool IsValid(out string error)
        {
            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context){}

        public override string UIDisplayText => $"has {Status}";
    }
}