using System;
using System.Diagnostics.CodeAnalysis;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters
{
    [Serializable]
    public struct AppliedModifier
    {
        [ConstrainedType(typeof(IModifier)), ValidateInput(nameof(IsIModifier), "Must be an IAction, skill or consumable")]
        [SerializeField] ScriptableObject _object;

        public double CreationTimeStamp;
        [MaybeNull] public CharacterTemplate Source;

        public IModifier Modifier
        {
            get => (IModifier)_object;
            set => _object = (ScriptableObject)value;
        }

        bool IsIModifier(ScriptableObject obj, ref string error)
        {
            if (obj is null)
            {
                error = "Must not be null";
                return false;
            }

            return obj is IModifier;
        }

        public AppliedModifier(EvaluationContext context, IModifier modifier, [MaybeNull] CharacterTemplate source)
        {
            _object = (ScriptableObject)modifier;
            CreationTimeStamp = context.CombatTimestamp;
            Source = source;
        }
    }
}