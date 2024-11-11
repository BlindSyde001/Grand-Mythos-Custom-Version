using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YNode;

namespace Screenplay.Nodes
{
    public class TrackStopper : Action
    {
        [Required, Input, SerializeReference] public TrackBackgroundPlayer? BackgroundPlayer;

        public override void CollectReferences(List<GenericSceneObjectReference> references) { }

        public override IEnumerable<Signal> Execute(IContext context)
        {
            if (BackgroundPlayer is null)
            {
                Debug.LogWarning($"Unassigned {nameof(BackgroundPlayer)}, skipping this {nameof(TrackStopper)}");
                yield return Signal.BreakInto(Next);
                yield break;
            }

            context.StopAsynchronous(BackgroundPlayer);
            yield return Signal.BreakInto(Next);
        }

        public override void FastForward(IContext context)
        {
            if (BackgroundPlayer is null)
            {
                Debug.LogWarning($"Unassigned {nameof(BackgroundPlayer)}, skipping this {nameof(TrackStopper)}");
                return;
            }

            context.StopAsynchronous(BackgroundPlayer);
        }

        public override void SetupPreview(IPreviewer previewer, bool fastForwarded)
        {
            if (BackgroundPlayer is null)
                return;

            previewer.StopAsynchronous(BackgroundPlayer);
        }
    }
}
