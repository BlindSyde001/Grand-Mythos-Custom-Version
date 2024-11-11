using System;
using System.Collections.Generic;
using UnityEngine;

namespace Screenplay
{
    [CreateAssetMenu(menuName = "Screenplay/Interlocutor")]
    public class Interlocutor : ScriptableObject
    {
        public float DefaultDuration = 0.1f;

        [SerializeField] private CharDuration[] _charDurations =
        {
            new CharDuration
            {
                Character = ' ',
                DurationInSeconds = 0f,
            },
            new CharDuration
            {
                Character = '\t',
                DurationInSeconds = 0f,
            },
            new CharDuration
            {
                Character = '\n',
                DurationInSeconds = 0f,
            },
            new CharDuration
            {
                Character = 'h',
                DurationInSeconds = 0f,
            },
            new CharDuration
            {
                Character = '.',
                DurationInSeconds = 1f,
            },
            new CharDuration
            {
                Character = ',',
                DurationInSeconds = 0.5f,
            },
            new CharDuration
            {
                Character = '!',
                DurationInSeconds = 1f,
            },
            new CharDuration
            {
                Character = '?',
                DurationInSeconds = 1f,
            }
        };

        public uint CharactersPerChatter = 4;
        public float Pitch = 1f;
        public float Volume = 1f;
        public AudioClip[] Chatter = Array.Empty<AudioClip>();

        private Dictionary<char, float> _charToDuration = null!;

        private void OnEnable()
        {
            _charToDuration = new();
            foreach (var charDuration in _charDurations)
                _charToDuration[charDuration.Character] = charDuration.DurationInSeconds;
        }

        public float GetDuration(char c)
        {
            if (_charToDuration.TryGetValue(c, out var duration) == false)
                if (_charToDuration.TryGetValue(char.IsLower(c) ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c), out duration) == false)
                    duration = DefaultDuration;

            return duration;
        }


        [Serializable]
        public struct CharDuration
        {
            public char Character;
            public float DurationInSeconds;
        }
    }
}
