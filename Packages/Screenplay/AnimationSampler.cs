using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Screenplay
{
    public class AnimationSampler : IDisposable
    {
        private readonly Animator _animator;
        private AnimationClipPlayable _playableClip;
        private PlayableGraph _graph;
        private bool _disposeOfAnimator;

        public AnimationSampler(AnimationClip clip, GameObject go)
        {
            _animator = go.GetComponent<Animator>();
            _disposeOfAnimator = false;
            if (_animator == null)
            {
                _disposeOfAnimator = true;
                _animator = go.AddComponent<Animator>();
                _animator.hideFlags |= HideFlags.DontSave | HideFlags.NotEditable;
            }

            Init(clip);
        }

        public AnimationSampler(AnimationClip clip, Animator animator, bool disposeOfAnimator)
        {
            _disposeOfAnimator = disposeOfAnimator;
            _animator = animator;
            Init(clip);
        }

        private void Init(AnimationClip clip)
        {
            _graph = PlayableGraph.Create();
            _graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            _playableClip = AnimationClipPlayable.Create(_graph, clip);
            var output = AnimationPlayableOutput.Create(_graph, nameof(AnimationSampler), _animator);
            output.SetSourcePlayable(_playableClip);
        }

        public void Sample(float previousTime, float t)
        {
            _playableClip.SetTime(t);
            _graph.Evaluate();
        }

        public void SampleAt(double time)
        {
            _playableClip.SetTime(time);
            _graph.Evaluate();
        }

        public void Dispose()
        {
            if (_graph.IsValid())
                _graph.Destroy();
            if (_disposeOfAnimator)
            {
                if (Application.isPlaying == false)
                    Object.DestroyImmediate(_animator);
                else
                    Object.Destroy(_animator);
            }
        }
    }
}
