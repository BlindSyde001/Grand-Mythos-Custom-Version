using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.VisualScripting;

namespace Interactables
{
    [Serializable]
    public class RunDialog : IInteraction
    {
        [Required]
        public ScriptMachine DialogScript;

        public IEnumerable<Delay> Interact(IInteractionSource source, OverworldPlayerController player)
        {
            EventBus.Trigger(new EventHook(DialogTriggeredEvent.Key, DialogScript.gameObject), DialogScript.gameObject);
            #warning this is a bit flacky, should change this into something more robust
            while (InputManager.CurrentState != GameState.Cutscene) // Wait for transition into dialog
                yield return Delay.WaitTillNextFrame;
            while (InputManager.CurrentState == GameState.Cutscene) // Wait for transition out of dialog
                yield return Delay.WaitTillNextFrame;
        }

        public bool IsValid(out string error)
        {
            if (DialogScript?.graph?.units.FirstOrDefault(x => x is DialogTriggeredEvent) == null)
                error = $"This component cannot trigger the dialog as {DialogScript}'s graph doesn't have a {nameof(DialogTriggeredEvent)}";

            error = null;
            return true;
        }
    }
}