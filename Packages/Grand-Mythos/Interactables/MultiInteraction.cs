using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Interactables
{
    [Serializable]
    public class MultiInteraction : IInteraction
    {
        [SerializeReference] public required IInteraction[] Array = System.Array.Empty<IInteraction>();
        public Mode Execution = Mode.Sequentially;

        public enum Mode
        {
            Sequentially,
            Simultaneously
        }

        public async UniTask InteractEnum(IInteractionSource source, OverworldPlayerController player)
        {
            switch (Execution)
            {
                case Mode.Sequentially:
                    foreach (var interaction in Array)
                        await interaction.InteractEnum(source, player);
                    break;
                case Mode.Simultaneously:
                    var enums = new UniTask[Array.Length];
                    for (int i = 0; i < enums.Length; i++)
                        enums[i] = Array[i].InteractEnum(source, player);

                    await UniTask.WhenAll(enums);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool IsValid([MaybeNullWhen(true)]out string error)
        {
            for (int i = 0; i < Array.Length; i++)
            {
                IInteraction? interaction = Array[i];
                if (interaction == null!)
                {
                    error = $"Interaction #{i} is null";
                    return false;
                }

                if (interaction.IsValid(out error) == false)
                    return false;
            }

            error = null;
            return true;
        }

        public void DuringSceneGui(IInteractionSource source, SceneGUIProxy sceneGUI)
        {
            foreach (var interaction in Array)
                interaction.DuringSceneGui(source, sceneGUI);
        }
    }
}