﻿using System;
using Sirenix.OdinInspector;

namespace Conditions
{
    [Serializable]
    public class AttributeBetween : SimplifiedCondition
    {
        [HorizontalGroup, HideLabel, SuffixLabel("<=")] public int Minimum;
        [HorizontalGroup, HideLabel, SuffixLabel("<=")] public Attribute TargetAttribute;
        [HorizontalGroup, HideLabel] public int Maximum;

        protected override bool Filter(CharacterTemplate target, EvaluationContext context)
        {
            int stat = target.GetAttribute(TargetAttribute);
            return Minimum <= stat && stat <= Maximum;
        }

        public override bool IsValid(out string error)
        {
            error = null;
            return true;
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context){}

        public override string UIDisplayText => $"has {TargetAttribute} between [{Minimum}, {Maximum}]";
    }
}