using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Screenplay;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace Characters
{
    public class NPC : MonoBehaviour
    {
        public required NavMeshAgent Agent;
        public required Animator Animator;
        public float WalkJogThreshold = 4f;
        public float JogRunThreshold = 6f;
        public float RotationSpeed = 2f;
        
        [ValidateInput(nameof(ValidateAnim))]
        [Tooltip("Plays the following state through the Animator")]
        public AnimationState Idle, Walk, Jog, Run;

        private bool _isIdle;
        private UniTaskCompletionSource? _moveTasks;

        private void Update()
        {
            if (Agent.remainingDistance > 0)
            {
                AnimationState selected;
                if (Agent.velocity.magnitude > JogRunThreshold)
                    selected = Run;
                else if (Agent.velocity.magnitude > WalkJogThreshold)
                    selected = Jog;
                else
                    selected = Walk;
                if (Animator.GetCurrentAnimatorStateInfo(selected.Layer).IsName(selected.StateName) == false)
                    Animator.Play(selected.StateName, selected.Layer);
                _isIdle = false;
            }
            else if (_isIdle == false)
            {
                _isIdle = true;
                Animator.Play(Idle.StateName, Idle.Layer);
            }
        }

        public async UniTask MoveTo(Vector3 pos, Quaternion rot, Movement movement, Cancellation cts)
        {
            var newTCS = new UniTaskCompletionSource();

            for (UniTaskCompletionSource? ongoing; (ongoing = Interlocked.CompareExchange(ref _moveTasks, newTCS, null)) != null;)
            {
                await ongoing.Task.AttachExternalCancellation(cts.GetStandardToken());
            }

            try
            {
                Agent.speed = movement switch
                {
                    Movement.Walk => WalkJogThreshold - 0.1f,
                    Movement.Jog => (WalkJogThreshold + JogRunThreshold) * 0.5f,
                    Movement.Run => JogRunThreshold + 0.1f,
                    _ => throw new ArgumentOutOfRangeException(nameof(movement), movement, null)
                };
                Agent.SetDestination(pos);
                while (Agent.pathPending)
                {
                    await Uni.NextFrame(cts, true);
                    cts.ThrowIfCancellationRequested();
                }

                while (Agent.hasPath)
                {
                    await Uni.NextFrame(cts, true);
                    cts.ThrowIfCancellationRequested();
                }

                var initialRotation = transform.rotation;
                for (float t = 0; t < 1f; t += Time.deltaTime * RotationSpeed)
                {
                    transform.rotation = Quaternion.Slerp(initialRotation, rot, Mathf.SmoothStep(0, 1, t));
                    await Uni.NextFrame(cts, true);
                    cts.ThrowIfCancellationRequested();
                }
            }
            finally
            {
                var r = Interlocked.Exchange(ref _moveTasks, null);
                Debug.Assert(r == newTCS);
                newTCS.TrySetResult();
            }
        }

        bool ValidateAnim(AnimationState state, ref string message)
        {
            if (Animator == null!)
            {
                message = $"{nameof(Animator)} is null";
                return false;
            }

            return state.EditorOnlyValidate(Animator, out message);
        }

        public enum Movement
        {
            Walk,
            Jog,
            Run,
        }
    }
}