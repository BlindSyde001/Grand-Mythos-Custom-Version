using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

namespace Interactables
{
    [Serializable]
    public class RunEvent : IInteraction
    {
        public UnityEvent? Event;

        public UniTask InteractEnum(IInteractionSource source, OverworldPlayerController player)
        {
            Event?.Invoke();
            return UniTask.CompletedTask;
        }

        public bool IsValid([MaybeNullWhen(true)] out string error)
        {
            error = null;
            return true;
        }

        public void DuringSceneGui(IInteractionSource source, SceneGUIProxy sceneGUI) { }
    }
}