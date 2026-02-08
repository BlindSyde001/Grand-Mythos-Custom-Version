using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters
{
    [Serializable]
    public struct AppliedModifier
    {
        [ConstrainedType(typeof(IModifier)), ValidateInput(nameof(IsIModifier), "Must be an IAction, skill or consumable")]
        [SerializeField] ScriptableObject? _object;
        [SerializeReference, ReadOnly] private IModifier? _modRef;

        public double CreationTimeStamp;
        public CharacterTemplate? Source;

        public IModifier Modifier
        {
            get => _modRef ?? (IModifier)_object!;
            set
            {
                _object = null!;
                _modRef = null;
                if (value is ScriptableObject so)
                    _object = so;
                else
                    _modRef = value;
            }
        }

        bool IsIModifier(ScriptableObject? obj, ref string error)
        {
            if (obj is null)
            {
                error = "Must not be null";
                return false;
            }

            return obj is IModifier;
        }

        public AppliedModifier(double combatTimestamp, IModifier modifier, CharacterTemplate? source)
        {
            _object = null;
            _modRef = null;
            CreationTimeStamp = combatTimestamp;
            Source = source;
            Modifier = modifier;
        }
    }
}