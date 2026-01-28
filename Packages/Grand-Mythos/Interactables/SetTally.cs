using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nodalog;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Interactables
{
    [Serializable]
    public class SetTally : IInteraction
    {
        [HorizontalGroup, HideLabel]
        public required Tally Tally;

        [HorizontalGroup, HideLabel, TableColumnWidth(10)]
        public OperationType Operation;

        [HorizontalGroup, HideLabel]
        public int Value = 1;

        public IEnumerable<Delay> InteractEnum(IInteractionSource source, OverworldPlayerController player)
        {
            Tally.Amount = Operation switch
            {
                OperationType.Set => Value,
                OperationType.Add => Tally.Amount + Value,
                OperationType.Sub => Tally.Amount - Value,
                _ => throw new ArgumentOutOfRangeException()
            };
            yield break;
        }

        public bool IsValid([MaybeNullWhen(true)] out string error)
        {
            error = null;
            return true;
        }

        public enum OperationType
        {
            [InspectorName("=")]
            Set,
            [InspectorName("+")]
            Add,
            [InspectorName("-")]
            Sub,
        }
    }
}