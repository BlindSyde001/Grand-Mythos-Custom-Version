using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Screenplay.Nodes
{
    public class PlayAnimation : Action
    {
        [Required] public SceneObjectReference<GameObject> Target;
        [Required] public AnimationClip? Clip = null;

        public override IEnumerable<Signal> Execute(IContext context)
        {
            if (Target.TryGet(out var go, out var failure) == false)
            {
                Debug.LogWarning($"Failed to {nameof(PlayAnimation)}, {nameof(Target)}: {failure}", context.Source);
                yield return Signal.BreakInto(Next);
                yield break;
            }
            if (Clip == null)
            {
                Debug.LogWarning($"Failed to {nameof(PlayAnimation)} on '{go}', {nameof(Clip)} is null", context.Source);
                yield return Signal.BreakInto(Next);
                yield break;
            }

            using var sampler = new AnimationSampler(Clip, go);
            float t = 0f;
            do
            {
                t += Time.deltaTime;
                t = t > Clip.length ? Clip.length : t;
                sampler.SampleAt(t);
                yield return Signal.NextFrame;
            } while (t < Clip.length);

            yield return Signal.BreakInto(Next);
        }

        public override void FastForward(IContext context)
        {
            if (Target.TryGet(out var go, out var failure) == false)
            {
                Debug.LogWarning($"Failed to {nameof(PlayAnimation)}, {nameof(Target)}: {failure}", context.Source);
                return;
            }
            if (Clip == null)
            {
                Debug.LogWarning($"Failed to {nameof(PlayAnimation)} on '{go}', {nameof(Clip)} is null", context.Source);
                return;
            }

            using var sampler = new AnimationSampler(Clip, go);
            sampler.SampleAt(Clip.length);
        }

        public override void SetupPreview(IPreviewer previewer, bool fastForwarded)
        {
            if (Target.TryGet(out var go, out _) == false || Clip == null)
                return;

            previewer.RegisterRollback(Clip, go);
            if (fastForwarded)
                FastForward(previewer);
            else
                previewer.PlaySafeAction(this);
        }

        public override void CollectReferences(List<GenericSceneObjectReference> references)
        {
            references.Add(Target);
        }
    }
}
