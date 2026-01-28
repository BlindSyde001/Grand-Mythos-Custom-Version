using System;
using System.Diagnostics.CodeAnalysis;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Conditions
{
    [Serializable]
    public class Or : Condition
    {
        [Title("v OR v", TitleAlignment = TitleAlignments.Centered), CustomValueDrawer("DrawNothing")]
        public DummyStruct _;

        [TabGroup("Left", TabName = "@this.Left?.UIDisplayText"), SerializeReference, HideLabel]
        public required Condition Left;
        [TabGroup("Right", TabName = "@this.Right?.UIDisplayText"), SerializeReference, HideLabel]
        public required Condition Right;

        protected override void FilterInner(ref TargetCollection targets, EvaluationContext context)
        {
            TargetCollection left = targets;
            TargetCollection right = targets;
            Left.Filter(ref left, context);
            Right.Filter(ref right, context);
            targets = left | right;
        }

        public override bool IsValid([MaybeNullWhen(true)] out string error)
        {
            if (Left == null!)
            {
                error = $"{nameof(Left)} is null";
                return false;
            }
            if (Right == null!)
            {
                error = $"{nameof(Right)} is null";
                return false;
            }

            return Left.IsValid(out error) && Right.IsValid(out error);
        }

        public override void NotifyUsedCondition(in TargetCollection target, EvaluationContext context)
        {
            Left.NotifyUsedCondition(target, context);
            Right.NotifyUsedCondition(target, context);
        }

        public override string UIDisplayText => $"{Left?.UIDisplayText} or {Right?.UIDisplayText}";

        [Serializable]
        public struct DummyStruct{}

        DummyStruct DrawNothing(DummyStruct value) => default;
    }
}