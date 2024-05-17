using System;
using System.Collections.Generic;
using Nodalog;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Interactables
{
    [Serializable]
    public class SetTally : IInteraction
    {
        [Required, HorizontalGroup, HideLabel]
        public Tally Tally;

        [Required, HorizontalGroup, HideLabel, TableColumnWidth(10)]
        public OperationType Operation;

        [Required, HorizontalGroup, HideLabel]
        public int Value = 1;

        public IEnumerable<Delay> Interact(IInteractionSource source, OverworldPlayerController player)
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

        public bool IsValid(out string error)
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