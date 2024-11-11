#nullable enable

using UnityEngine;
using Screenplay.Nodes;

namespace Screenplay.Component
{
    public abstract class UIBase : MonoBehaviour
    {
        public abstract bool DialogAdvancesAutomatically { get; }
        public abstract bool FastForwardRequested { get; }
        public abstract void StartLineTypewriting(string line);
        public abstract void FinishedTypewriting();
        public abstract void SetTypewritingCharacter(int characters);
        public abstract void StartDialogPresentation();
        public abstract void EndDialogPresentation();
        public abstract SelectedChoice ChoicePresentation(Choice.Data[] choices);
        public abstract void PlayChatter(AudioClip clip, Interlocutor interlocutor);
    }

    public class SelectedChoice
    {
        /// <summary>
        /// Null as long as nothing is selected
        /// </summary>
        public Choice.Data? Selection;
    }
}
