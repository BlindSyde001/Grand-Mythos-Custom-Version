using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ActionAnimation
{
    public class PlayAnimator : IActionAnimation
    {
        [Tooltip("Plays the following state through the BattlePrefab's Animator")]
        public string StateName;
        public int Layer;

        public IEnumerable Play(IAction action, BattleCharacterController controller, BattleCharacterController[] targets)
        {
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

            yield return new WaitForSeconds(GetTimeLeft(current));
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
}