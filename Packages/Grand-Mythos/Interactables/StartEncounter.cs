using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Interactables
{
    [Serializable]
    public class StartEncounter : IInteraction
    {
        [SerializeReference] public required IEncounterDefinition Encounter;

        public async UniTask InteractEnum(IInteractionSource source, OverworldPlayerController player)
        {
            await Encounter.Start(CancellationToken.None);
            #warning this is a bit flacky, should change this into something more robust
            while (InputManager.CurrentState == GameState.Battle) // Wait for transition out of battle
                await UniTask.NextFrame();
        }

        public bool IsValid([MaybeNullWhen(true)] out string error)
        {
            if (Encounter == null!)
            {
                error = $"{nameof(Encounter)} is null";
                return false;
            }

            return Encounter.IsValid(out error);
        }

        public void DuringSceneGui(IInteractionSource source, SceneGUIProxy sceneGUI) { }
    }
}