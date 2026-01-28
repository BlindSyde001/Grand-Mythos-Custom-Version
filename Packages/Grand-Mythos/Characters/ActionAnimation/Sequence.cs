using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ActionAnimation
{
    public class Sequence : IActionAnimation
    {
        [Tooltip("Plays the following animations in sequence; when the first one finishes, the second starts.")]
        [SerializeReference]
        public IActionAnimation[] Animations = Array.Empty<IActionAnimation>();

        public async UniTask Play(IAction? action, BattleCharacterController controller, BattleCharacterController[] targets, CancellationToken cancellation)
        {
            foreach (var animation in Animations)
                await animation.Play(action, controller, targets, cancellation);
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