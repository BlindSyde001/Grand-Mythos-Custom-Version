using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using QTE;
using UnityEngine;

namespace ActionAnimation
{
    public class Simultaneous : IActionAnimation
    {
        [Tooltip("Plays the following animations simultaneously; all of them start and play together at the same time")]
        [SerializeReference]
        public IActionAnimation[] Animations = Array.Empty<IActionAnimation>();

        public async IAsyncEnumerable<(QTEStart qte, double start, float duration)> Play(IAction action, BattleCharacterController controller, BattleCharacterController[] targets, [EnumeratorCancellation] CancellationToken cancellation)
        {
            int amount = Animations.Length;
            var channel = Channel.CreateSingleConsumerUnbounded<(QTEStart qte, double start, float duration)>();
            foreach (var animation in Animations)
                _ = Wrapper(animation);

            await foreach (var v in channel.Reader.ReadAllAsync(cancellation))
            {
                yield return v;
            }

            async Task Wrapper(IActionAnimation animation)
            {
                await foreach (var qteData in animation.Play(action, controller, targets, cancellation))
                    channel.Writer.TryWrite(qteData);

                if (Interlocked.Decrement(ref amount) == 0)
                    channel.Writer.Complete();
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