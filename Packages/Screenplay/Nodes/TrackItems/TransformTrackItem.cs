using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Screenplay.Nodes.TrackItems
{
    [Serializable]
    public class TransformTrackItem : ITrackItem
    {
        [Required, HorizontalGroup, HideLabel] public SceneObjectReference<Transform> Target;
        [SerializeField, HorizontalGroup, HideLabel, SuffixLabel("Start")] private float _start;
        [SerializeField, HorizontalGroup, HideLabel, SuffixLabel("Duration")] private float _duration = 1f;
        [HorizontalGroup("G2")] public Vector3 Position;
        [HorizontalGroup("G2")] public Quaternion Rotation = Quaternion.identity;
        [HorizontalGroup("G2")] public EasingFunction Easing = EasingFunction.InOutCubic;

        public string Label
        {
            get
            {
                if (Target.TryGet(out var obj, out _))
                    return $"Transform {obj.name}";
                else
                    return $"Transform UNLOADED";
            }
        }

        public float Start
        {
            get => _start;
            set => _start = value;
        }

        public float Duration
        {
            get => _duration;
            set => _duration = value;
        }

        public void CollectReferences(List<GenericSceneObjectReference> references) => references.Add(Target);

        public ITrackSampler? TryGetSampler()
        {
            if (Target.TryGet(out var go, out var failure) == false)
            {
                Debug.LogWarning($"Failed to generate sampler for {this}, {nameof(Target)}: {failure}");
                return null;
            }

            return new Sampler(go, Position, Rotation, Start, _duration, Easing);
        }

        public void AppendRollbackMechanism(IPreviewer previewer)
        {
            if (Target.TryGet(out var go, out var failure) == false)
            {
                return;
            }

            var startPosition = go.transform.position;
            var startRotation = go.transform.rotation;
            previewer.RegisterRollback(() =>
            {
                go.transform.position = startPosition;
                go.transform.rotation = startRotation;
            });
        }

        public void DrawGizmos(ref bool rebuildPreview)
        {
#if UNITY_EDITOR
            var newPosition = UnityEditor.Handles.PositionHandle(Position, Rotation);
            var newRotation = UnityEditor.Handles.RotationHandle(Rotation, Position);
            rebuildPreview |= newPosition != Position || newRotation != Rotation;
            Position = newPosition;
            Rotation = newRotation;
#endif
        }

        private class Sampler : ITrackSampler
        {
            private Transform _go;
            private float _start, _duration;
            private Vector3 _startPosition, _endPosition;
            private Quaternion _startRotation, _endRotation;
            private EasingFunction _easing;

            public Sampler(Transform go, Vector3 endPos, Quaternion endRot, float start, float duration, EasingFunction easing)
            {
                _go = go;
                _start = start;
                _duration = duration;

                _startPosition = go.transform.position;
                _startRotation = go.transform.rotation;
                _endPosition = endPos;
                _endRotation = endRot;

                _easing = easing;
            }

            public void Dispose() { }

            public void Sample(float previousTime, float t)
            {
                const float c1 = 1.70158f;
                const float c2 = c1 * 1.525f;
                const float c3 = c1 + 1;
                const float c4 = 2 * MathF.PI / 3;
                const float n1 = 7.5625f;
                const float d1 = 2.75f;

                t = (t - _start) / _duration;
                t = t > 1 ? 1 : t < 0 ? 0 : t;
                float x = t;
                t = _easing switch
                {
                    // https://easings.net/
                    EasingFunction.Linear => t,
                    EasingFunction.InCubic => t*t*t,
                    EasingFunction.OutCubic => 1 - MathF.Pow(1 - t, 3),
                    EasingFunction.InOutCubic => t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f,
                    EasingFunction.InBack => c3 * x * x * x - c1 * x * x,
                    EasingFunction.OutBack => 1 + c3 * MathF.Pow(t - 1, 3) + c1 * MathF.Pow(t - 1, 2),
                    EasingFunction.InOutBack => x switch
                    {
                        < 0.5f => (MathF.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2,
                        _ => (MathF.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2
                    },
                    EasingFunction.OutElastic => x switch
                    {
                        0 => 0,
                        1 => 1,
                        _ => MathF.Pow(2, -10 * x) * MathF.Sin((x * 10 - 0.75f) * c4) + 1
                    },
                    EasingFunction.OutBounce => x switch
                    {
                        < 1 / d1 => n1 * x * x,
                        < 2 / d1 => n1 * (x -= 1.5f / d1) * x + 0.75f,
                        < 2.5f / d1 => n1 * (x -= 2.25f / d1) * x + 0.9375f,
                        _ => n1 * (x -= 2.625f / d1) * x + 0.984375f
                    },
                    _ => throw new ArgumentOutOfRangeException()
                };
                _go.transform.position = Vector3.LerpUnclamped(_startPosition, _endPosition, t);
                _go.transform.rotation = Quaternion.SlerpUnclamped(_startRotation, _endRotation, t);
            }
        }

        public enum EasingFunction
        {
            Linear,
            InCubic,
            OutCubic,
            InOutCubic,
            InBack,
            OutBack,
            InOutBack,
            OutElastic,
            OutBounce
        }
    }
}
