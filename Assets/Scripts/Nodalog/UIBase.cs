#nullable enable

using System.Threading.Tasks;
using UnityEngine;

namespace Nodalog
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
        public abstract Task<ChoiceData> ChoicePresentation(ChoiceData[] choices);
        public abstract void PlayChatter(AudioClip clip, Interlocutor interlocutor);
    }
}