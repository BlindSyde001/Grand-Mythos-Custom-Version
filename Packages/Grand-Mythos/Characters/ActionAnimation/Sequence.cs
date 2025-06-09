using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using JetBrains.Annotations;
using QTE;
using UnityEngine;

namespace ActionAnimation
{
    public class Sequence : IActionAnimation
    {
        [Tooltip("Plays the following animations in sequence; when the first one finishes, the second starts.")]
        [SerializeReference]
        public IActionAnimation[] Animations = Array.Empty<IActionAnimation>();

        public async IAsyncEnumerable<(QTEStart qte, double start, float duration)> Play(IAction action, BattleCharacterController controller, BattleCharacterController[] targets, [EnumeratorCancellation] CancellationToken cancellation)
        {
            foreach (var animation in Animations)
            {
                await foreach (var qteData in animation.Play(action, controller, targets, cancellation))
                {
                    yield return qteData;
                }
            }
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