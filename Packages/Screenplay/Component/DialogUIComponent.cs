using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Screenplay.Nodes;

namespace Screenplay.Component
{
    public class DialogUIComponent : UIBase
    {
        private static readonly SortedSet<SourceEndPair> s_sourcesPool = new();

        public AudioMixerGroup? MixerGroup;
        public bool AdvancesAutomatically = false;
        [SerializeReference] public IFastForward FastForward = new InputFastForward();
        public DialogChoiceTemplate DialogChoiceTemplate = null!;
        public UnityEvent<string>? OnNewLine;
        public UnityEvent? OnLineFullyVisible;
        public UnityEvent<int>? SetHowManyCharactersAreVisible;
        public UnityEvent? OnStart, OnEnd;
        public UnityEvent? OnChoicePresented, OnChoiceClosed;

        public override bool DialogAdvancesAutomatically => AdvancesAutomatically;
        public override bool FastForwardRequested => FastForward.IsRequesting();
        public override void StartLineTypewriting(string line) => OnNewLine?.Invoke(line);
        public override void FinishedTypewriting() => OnLineFullyVisible?.Invoke();
        public override void SetTypewritingCharacter(int characters) => SetHowManyCharactersAreVisible?.Invoke(characters);
        public override void StartDialogPresentation() => OnStart?.Invoke();
        public override void EndDialogPresentation() => OnEnd?.Invoke();

        public override SelectedChoice ChoicePresentation(Choice.Data[] choices)
        {
            DialogChoiceTemplate.gameObject.SetActive(false);
            OnChoicePresented?.Invoke();
            var awaitableSource = new SelectedChoice();
            var choiceGameObjects = new List<GameObject>();
            foreach (var choice in choices)
            {
                if (choice.Enabled == false)
                    continue;

                var uiChoice = Instantiate(DialogChoiceTemplate, DialogChoiceTemplate.transform.parent);
                choiceGameObjects.Add(uiChoice.gameObject);
                uiChoice.gameObject.SetActive(true);
                uiChoice.Label.text = choice.Text;
                uiChoice.Button.interactable = choice.Enabled;
                uiChoice.Button.onClick.AddListener(() =>
                {
                    awaitableSource.Selection = choice;
                    foreach (var go in choiceGameObjects)
                        Destroy(go);
                    OnChoiceClosed?.Invoke();
                });
            }
            return awaitableSource;
        }

        public override void PlayChatter(AudioClip chatter, Interlocutor interlocutor)
        {
            AudioSource source;
            if (s_sourcesPool.Count > 0 && s_sourcesPool.Min.End < Time.timeAsDouble)
            {
                source = s_sourcesPool.Min.Source;
                s_sourcesPool.Remove(s_sourcesPool.Min);
            }
            else
            {
                source = new GameObject($"{nameof(DialogUIComponent)}PooledAudio").AddComponent<AudioSource>();
            }

            source.spatialize = false;
            source.clip = chatter;
            source.pitch = interlocutor.Pitch;
            source.volume = interlocutor.Volume;
            source.outputAudioMixerGroup = MixerGroup;
            source.Play();
            s_sourcesPool.Add(new()
            {
                End = Time.timeAsDouble + chatter.length,
                Source = source
            });
        }

        private void Awake()
        {
            DialogChoiceTemplate.gameObject.SetActive(false);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearPostReload()
        {
            s_sourcesPool.Clear();
        }

        public interface IFastForward
        {
            bool IsRequesting();
        }

        [Serializable]
        public class InputFastForward : IFastForward
        {
            public InputActionReference Input = null!;

            public bool IsRequesting()
            {
                return Input.action.WasPerformedThisFrame();
            }
        }

        public struct SourceEndPair : IComparable<SourceEndPair>, IEquatable<SourceEndPair>
        {
            public double End;
            public AudioSource Source;

            public int CompareTo(SourceEndPair other) => End.CompareTo(other.End);

            public bool Equals(SourceEndPair other) => End.Equals(other.End) && Source.Equals(other.Source);

            public override bool Equals(object? obj) => obj is SourceEndPair other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(End, Source);
        }
    }
}
