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
        
        [ValidateInput(nameof(ValidateAnim))]
        [Tooltip("Plays the following state through the Animator")]
        public AnimationState Idle, Walk, Run;

        private bool _isIdle;

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

        /*public IEnumerable Play(IAction action, BattleCharacterController controller, BattleCharacterController[] targets)
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

            controller.Animator.Play(StateName, Layer);

            yield return null; // Can't get current state until one frame passes ...

            var current = controller.Animator.GetCurrentAnimatorStateInfo(Layer);

            if (count != 0)
            {
                averagePos /= count;
                averagePos.y = 0;
                controller.transform.DOLookAt(averagePos, 0.25f);
                yield return new WaitForSeconds(0.25f);
            }

            if (current.IsName(StateName) == false)
            {
                Debug.LogError($"Error while playing state {StateName} on layer {Layer}, does that state exist on that layer for animator '{controller.Animator.runtimeAnimatorController}'", controller.Animator);
                yield break;
            }

            var timeLeft = GetTimeLeft(current);
            yield return new WaitForSeconds(timeLeft);

            if (count != 0)
            {
                controller.transform.DOLookAt(controller.transform.position + initialRotation * Vector3.forward, 0.25f);
                yield return new WaitForSeconds(0.25f);
            }
        }

        static float GetTimeLeft(in AnimatorStateInfo info)
        {
            return info.length * (1f - info.normalizedTime) / info.speed / info.speedMultiplier;
        }*/

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