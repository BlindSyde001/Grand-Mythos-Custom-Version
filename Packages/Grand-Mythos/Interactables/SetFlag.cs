using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using Nodalog;
using Sirenix.OdinInspector;

namespace Interactables
{
    [Serializable]
    public class SetFlag : IInteraction
    {
        [HorizontalGroup, HideLabel]
        public required Flag Flag;

        [HorizontalGroup, LabelText(" \u2192 ")]
        public bool NewState;

        public UniTask InteractEnum(IInteractionSource source, OverworldPlayerController player)
        {
            Flag.State = NewState;
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