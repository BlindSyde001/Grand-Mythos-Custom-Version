using System.Collections.Generic;
using Screenplay.Nodes;
using UnityEngine;

namespace Screenplay
{
    public interface IPreviewer : IContext
    {
        bool Loop { get; }
        void RegisterRollback(System.Action rollback);
        void RegisterRollback(AnimationClip clip, GameObject go);
        void PlayCustomSignal(IEnumerable<Signal> signal);

        /// <summary>
        /// <see cref="Signal.DelayType.SwapToAction"/> will be ignored
        /// </summary>
        void PlaySafeAction(IAction action)
        {
            PlayCustomSignal(PreviewPlay());

            IEnumerable<Signal> PreviewPlay()
            {
                do
                {
                    foreach (var signal in action.Execute(this))
                    {
                        if (signal.Type is Signal.DelayType.SwapToAction or Signal.DelayType.SoftBreak)
                            break;
                        yield return signal;
                    }

                    if (Loop)
                    {
                        for (float f = 0f; f < 1f; f += Time.deltaTime)
                        {
                            yield return Signal.NextFrame;
                        }
                    }
                } while (Loop);
            }
        }
    }
}
