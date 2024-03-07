using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ActionAnimation
{
    public class Simultaneous : IActionAnimation
    {
        [InfoBox("Plays the following animations simultaneously; all of them start and play together at the same time")]
        [SerializeReference]
        public IActionAnimation[] Animations = Array.Empty<IActionAnimation>();

        public IEnumerable Play(IAction action, BattleCharacterController controller, TargetCollection targets)
        {
            if (BattleStateMachine.TryGetInstance(out var bts) == false)
                yield break;

            int[] runningCounter = new int[1]; // Using an array here to share read and writes
            foreach (var animation in Animations)
            {
                runningCounter[0]++;
                // Have to run it through StartCoroutine since there is no clean way to handle concurrent WaitForSeconds and other YieldInstructions
                bts.StartCoroutine(PlayWrapper(animation, action, controller, targets, runningCounter).GetEnumerator());
            }

            while (runningCounter[0] != 0)
                yield return null;
        }

        IEnumerable PlayWrapper(IActionAnimation animation, IAction action, BattleCharacterController controller, TargetCollection targets, int[] running)
        {
            foreach (var yield in animation.Play(action, controller, targets))
                yield return yield;
            running[0]--;
        }

        public bool Validate([MaybeNull]IAction action, CharacterTemplate template, ref string message)
        {
            foreach (var actionAnimation in Animations)
            {
                if (actionAnimation.Validate(action, template, ref message) == false)
                    return false;
            }

            return true;
        }
    }
}