using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Interactables.Conditions
{
    [Serializable]
    public class And : ICondition
    {
        [HorizontalGroup, SerializeReference, HideLabel, SuffixLabel(" AND ")]
        public required ICondition A;
        [HorizontalGroup, SerializeReference, HideLabel, SuffixLabel("?")]
        public required ICondition B;

        public bool Evaluate() => A.Evaluate() && B.Evaluate();

        public bool IsValid(out string error)
        {
            if (A == null!)
            {
                error = $"{nameof(A)} is null";
                return false;
            }

            if (B == null!)
            {
                error = $"{nameof(B)} is null";
                return false;
            }

            if (A.IsValid(out error) == false || B.IsValid(out error) == false)
                return false;

            error = "";
            return true;
        }
    }
}