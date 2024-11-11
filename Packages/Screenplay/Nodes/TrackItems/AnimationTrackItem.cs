using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Screenplay.Nodes.TrackItems
{
    [Serializable]
    public class AnimationTrackItem : ITrackItem
    {
        [Required, HorizontalGroup, HideLabel] public SceneObjectReference<GameObject> Target;
        [Required, HorizontalGroup, HideLabel] public AnimationClip? Clip;
        [SerializeField, HorizontalGroup, HideLabel, SuffixLabel("Start")] private float _start;
        [SerializeField, HorizontalGroup, HideLabel, SuffixLabel("Duration Mult")] private float _durationMult = 1f;

        public float Start
        {
            get => _start;
            set => _start = value;
        }

        public float Duration
        {
            get => Clip == null ? _start : Clip.length * _durationMult;
            set
            {
                if (Clip == null)
                    return;

                _durationMult = value / Clip.length;
            }
        }

        public string Label
        {
            get
            {
                string clipName = Clip == null ? "NULL" : Clip.name;
                if (Target.TryGet(out var obj, out _))
                    return $"{obj.name} -> {clipName}";
                else
                    return $"UNLOADED -> {clipName}";
            }
        }

        public void CollectReferences(List<GenericSceneObjectReference> references) => references.Add(Target);

        public ITrackSampler? TryGetSampler()
        {
            if (Target.TryGet(out var go, out var failure) == false)
            {
                Debug.LogWarning($"Failed to generate sampler for {nameof(AnimationTrackItem)}, {nameof(Target)}: {failure}");
                return null;
            }
            if (Clip == null)
            {
                Debug.LogWarning($"Failed to generate sampler for {nameof(AnimationTrackItem)} on '{go}', {nameof(Clip)} is null");
                return null;
            }

            return new AnimationTrackSampler(Clip, go, _start, _durationMult);
        }

        public void AppendRollbackMechanism(IPreviewer previewer)
        {
            if (Target.TryGet(out var go, out var failure) == false || Clip == null)
                return;

            previewer.RegisterRollback(Clip, go);
        }

        private class AnimationTrackSampler : ITrackSampler
        {
            private AnimationSampler _trackSampler;
            private float _start, _durationMult;
            private AnimationClip _clip;

            public AnimationTrackSampler(AnimationClip clip, GameObject go, float start, float durationMult)
            {
                _clip = clip;
                _trackSampler = new(clip, go);
                _start = start;
                _durationMult = durationMult;
            }

            public void Dispose()
            {
                _trackSampler.Dispose();
            }

            public void Sample(float previousTime, float t)
            {
                t = (t - _start) / _durationMult;
                t = t > _clip.length ? _clip.length : t < 0 ? 0 : t;
                previousTime = (previousTime - _start) / _durationMult;
                previousTime = previousTime > _clip.length ? _clip.length : previousTime < 0 ? 0 : previousTime;
                _trackSampler.Sample(previousTime, t);
            }
        }
    }
}
