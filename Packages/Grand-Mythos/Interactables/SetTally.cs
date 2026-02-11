using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
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

        public UniTask InteractEnum(IInteractionSource source, OverworldPlayerController player)
        {
            Tally.Amount = Operation switch
            {
                OperationType.Set => Value,
                OperationType.Add => Tally.Amount + Value,
                OperationType.Sub => Tally.Amount - Value,
                _ => throw new ArgumentOutOfRangeException()
            };
            return UniTask.CompletedTask;
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

        public void DuringSceneGui(IInteractionSource source, SceneGUIProxy sceneGUI) { }
    }
}