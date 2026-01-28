using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Interactables
{
    [Serializable]
    public class StartEncounter : IInteraction
    {
        [SerializeReference] public required IEncounterDefinition Encounter;

        public IEnumerable<Delay> InteractEnum(IInteractionSource source, OverworldPlayerController player)
        {
            Encounter.Start(player);
            #warning this is a bit flacky, should change this into something more robust
            while (InputManager.CurrentState != GameState.Battle) // Wait for transition into battle
                yield return Delay.WaitTillNextFrame;
            while (InputManager.CurrentState == GameState.Battle) // Wait for transition out of battle
                yield return Delay.WaitTillNextFrame;
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
    }
}