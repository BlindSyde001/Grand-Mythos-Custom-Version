using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ActionAnimation
{
    public class Simultaneous : IActionAnimation
    {
        [Tooltip("Plays the following animations simultaneously; all of them start and play together at the same time")]
        [SerializeReference]
        public IActionAnimation[] Animations = Array.Empty<IActionAnimation>();

        public async UniTask Play(IAction? action, BattleCharacterController controller, BattleCharacterController[] targets, CancellationToken cancellation)
        {
            var tasks = new List<UniTask>(Animations.Length);
            foreach (var animation in Animations)
                tasks.Add(animation.Play(action, controller, targets, cancellation));

            await UniTask.WhenAll(tasks);
        }

        public bool Validate([CanBeNull]IAction action, CharacterTemplate template, ref string message)
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