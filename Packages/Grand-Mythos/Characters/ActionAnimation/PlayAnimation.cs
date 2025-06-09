using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using JetBrains.Annotations;
using QTE;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ActionAnimation
{
    public class PlayAnimator : IActionAnimation
    {
        [Tooltip("Plays the following state through the BattlePrefab's Animator")]
        public string StateName;
        public int Layer;

        public async IAsyncEnumerable<(QTEStart qte, double start, float duration)> Play(IAction action, BattleCharacterController controller, BattleCharacterController[] targets, [EnumeratorCancellation] CancellationToken cancellation)
        {
            var initialRotation = controller.transform.rotation;
            Vector3 averagePos = default;
            int count = 0;
            foreach (var target in targets)
            {
                if (target == controller)
                    continue;

                count++;
                averagePos += target.transform.position;
            }

            var hash = Animator.StringToHash(StateName);
            if (controller.Animator.HasState(Layer, hash) == false)
            {
                Debug.LogError($"Error while playing state {StateName} on layer {Layer}, does that state exist on that layer for animator '{controller.Animator.runtimeAnimatorController}'", controller.Animator);
                yield break;
            }

            controller.Animator.Play(hash, Layer);

            var callback = controller.gameObject.AddComponent<AnimationEventCallback>();
            var channel = Channel.CreateSingleConsumerUnbounded<(QTEStart qte, double start, float duration)>();
            try
            {
                callback.Handler = (qte, startTimestamp, duration) =>
                {
                    if (cancellation.IsCancellationRequested)
                        return;
                    var current = controller.Animator.GetCurrentAnimatorStateInfo(Layer);
                    channel.Writer.TryWrite((qte, startTimestamp, duration / current.speed / current.speedMultiplier));
                };

                await UniTask.Yield(cancellation); // Can't get current state until one frame passes ...

                var stateInfo = controller.Animator.GetCurrentAnimatorStateInfo(Layer);

                if (stateInfo.IsName(StateName) == false)
                {
                    Debug.LogWarning($"Animation {StateName} @ {Layer} on {controller} was likely interrupted", controller);
                    yield break; // Something interrupted this animation
                }

                if (count != 0)
                {
                    averagePos /= count;
                    averagePos.y = 0;
                    controller.transform.DOLookAt(averagePos, 0.25f);
                }

                var timeLeft = GetTimeLeft(stateInfo);
                if (timeLeft > 0)
                {
                    _ = WaitAndClose(timeLeft, cancellation);

                    await foreach (var data in channel.Reader.ReadAllAsync(cancellation))
                    {
                        yield return data;
                    }
                }
            }
            finally
            {
                channel.Writer.TryComplete();
                Object.Destroy(callback);
            }

            if (count != 0)
            {
                controller.transform.DOLookAt(controller.transform.position + initialRotation * Vector3.forward, 0.25f);
                await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: cancellation);
            }

            async UniTask WaitAndClose(float duration, CancellationToken token)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);
                channel.Writer.Complete();
            }
        }

        static float GetTimeLeft(in AnimatorStateInfo info)
        {
            return info.length * (1f - info.normalizedTime) / info.speed / info.speedMultiplier;
        }

        public bool Validate([CanBeNull]IAction action, CharacterTemplate template, ref string message)
        {
            if (template.BattlePrefab == null)
            {
                message = $"{nameof(template.BattlePrefab)} is null";
                return false;
            }

            if (template.BattlePrefab.GetComponentInChildren<BattleCharacterController>() is {} controller == false)
            {
                message = $"{nameof(template.BattlePrefab)} does not have any {nameof(BattleCharacterController)}";
                return false;
            }

            if (controller.Animator == null)
            {
                message = $"{nameof(template.BattlePrefab)} does not have its {nameof(controller.Animator)} set";
                return false;
            }

            return new AnimationState { StateName = StateName, Layer = Layer }.EditorOnlyValidate(controller.Animator, out message);
        }
    }

    public class AnimationEventCallback : MonoBehaviour
    {
        [CanBeNull] public Action<QTEStart, double, float> Handler;

        public void HandleQTEEvent(AnimationEvent animationEvent)
        {
            if (animationEvent.objectReferenceParameter is QTEStart qte)
            {
                /*var overrun = animationEvent.animationState.time - animationEvent.time;
                overrun /= animationEvent.animationState.speed;*/
                
                Handler?.Invoke(qte, Time.timeAsDouble/* - overrun*/, animationEvent.floatParameter);
            }
        }
    }
}