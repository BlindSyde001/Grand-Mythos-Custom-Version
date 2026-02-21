using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace Characters
{
    public class NPC : MonoBehaviour
    {
        public required NavMeshAgent Agent;
        public required Animator Animator;
        public float WalkRunThreshold = 2f;
        public float RotationSpeed = 2f;
        
        [ValidateInput(nameof(ValidateAnim))]
        [Tooltip("Plays the following state through the Animator")]
        public AnimationState Idle, Walk, Run;

        private bool _isIdle;
        private UniTaskCompletionSource? _moveTasks;

        private void Update()
        {
            if (Agent.remainingDistance > 0)
            {
                var selected = Agent.velocity.magnitude > WalkRunThreshold ? Run : Walk;
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

        public async UniTask MoveTo(Vector3 pos, Quaternion rot, CancellationToken cts)
        {
            var newTCS = new UniTaskCompletionSource();

            for (UniTaskCompletionSource? ongoing; (ongoing = Interlocked.CompareExchange(ref _moveTasks, newTCS, null)) != null;)
            {
                await ongoing.Task.WithInterruptingCancellation(cts);
            }

            try
            {
                Agent.SetDestination(pos);
                while (Agent.pathPending)
                {
                    await UniTask.NextFrame(cts, true);
                    cts.ThrowIfCancellationRequested();
                }

                while (Agent.hasPath)
                {
                    await UniTask.NextFrame(cts, true);
                    cts.ThrowIfCancellationRequested();
                }

                var initialRotation = transform.rotation;
                for (float t = 0; t < 1f; t += Time.deltaTime * RotationSpeed)
                {
                    transform.rotation = Quaternion.Slerp(initialRotation, rot, Mathf.SmoothStep(0, 1, t));
                    await UniTask.NextFrame(cts, true);
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
    }
}