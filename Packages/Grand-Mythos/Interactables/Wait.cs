using System;
using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;

namespace Interactables
{
    [Serializable]
    public class Wait : IInteraction
    {
        public float DurationInSeconds = 1f;
        public UniTask InteractEnum(IInteractionSource source, OverworldPlayerController player)
        {
            return UniTask.Delay(TimeSpan.FromSeconds(DurationInSeconds));
        }

        public bool IsValid([MaybeNullWhen(true)] out string error)
        {
            if (DurationInSeconds < 0)
            {
                error = $"{nameof(DurationInSeconds)} is negative ({DurationInSeconds})";
                return false;
            }

            error = null;
            return true;
        }

        public void DuringSceneGui(IInteractionSource source, SceneGUIProxy sceneGUI) { }
    }
}