﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ActionAnimation
{
    public class PlayAnimator : IActionAnimation
    {
        [InfoBox("Plays the following state through the BattlePrefab's Animator")]
        public string StateName;
        public int Layer;

        public IEnumerable Play(IAction action, BattleCharacterController controller, TargetCollection targets)
        {
            Vector3 averagePos = default;
            int count = 0;
            foreach (var target in targets)
            {
                count++;
                averagePos += target.Context.Controller.transform.position;
            }

            if (count != 0)
            {
                averagePos /= count;

                var start = controller.transform.rotation;
                var dir = averagePos - controller.transform.position;
                dir.y = 0;
                var end = Quaternion.LookRotation(Vector3.Normalize(dir), Vector3.up);
                for (float f = 0f; f < 1f; f += Time.deltaTime)
                {
                    controller.transform.rotation = Quaternion.Slerp(start, end, f);
                    yield return null;
                }

                controller.transform.rotation = end;
            }

            controller.Animator.Play(StateName, Layer);

            yield return null; // Can't get current state until one frame passes ...

            var current = controller.Animator.GetCurrentAnimatorStateInfo(Layer);

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

        public bool Validate(IAction action, CharacterTemplate template, ref string message)
        {
            if (template.BattlePrefab == null)
            {
                message = $"{nameof(template.BattlePrefab)} is null";
                return false;
            }

            if (template.BattlePrefab.GetComponentInChildren<Animator>() is {} animator == false)
            {
                message = $"{nameof(template.BattlePrefab)} does not have any {nameof(Animator)}";
                return false;
            }

            if (animator.runtimeAnimatorController == null)
            {
                message = $"The {nameof(template.BattlePrefab)} {nameof(Animator)}'s controller is null";
                return false;
            }

#if UNITY_EDITOR
            var editTimeController = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(UnityEditor.AssetDatabase.GetAssetPath(animator.runtimeAnimatorController));
            if (Layer >= 0 && Layer < editTimeController.layers.Length)
            {
                foreach (var state in editTimeController.layers[Layer].stateMachine.states)
                {
                    if (state.state.name == StateName)
                        return true;
                }
            }

            List<string> strings = new();
            for (int i = 0; i < editTimeController.layers.Length; i++)
            {
                foreach (var state in editTimeController.layers[i].stateMachine.states)
                    strings.Add($"{i}: {state.state.name}");
            }

            message = $"Animator '{animator.runtimeAnimatorController.name}' in this {nameof(template.BattlePrefab)} does not have a state named '{StateName}'. States: \n{string.Join(" | ", strings.OrderBy(x => x))}";
            Debug.LogWarning($"{message}, click on me to navigate to that controller", animator.runtimeAnimatorController);
            return false;
#else
            return true;
#endif
        }
    }
}