using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Interactables
{
    [Serializable]
    public class StartEncounter : IInteraction
    {
        [Required, SerializeReference] public IEncounterDefinition Encounter;

        public IEnumerable<Delay> Interact(IInteractionSource source, OverworldPlayerControlsNode player)
        {
            Encounter.Start(source.transform, player);
            yield break;
        }

        public bool IsValid(out string error)
        {
            if (Encounter == null)
            {
                error = $"{nameof(Encounter)} is null";
                return false;
            }

            return Encounter.IsValid(out error);
        }
    }
}