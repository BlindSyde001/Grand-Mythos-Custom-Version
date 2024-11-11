using System.Collections.Generic;
using UnityEngine;

namespace Screenplay.Nodes
{
    public class TrackBackgroundPlayer : TrackPlayer
    {
        public bool Loop;

        private IEnumerable<Signal> AsyncRunner(Track track)
        {
            foreach (var signal in track.RangePlayer(GetTimeSpan(track), Loop))
                yield return signal;
        }

        public override IEnumerable<Signal> Execute(IContext context)
        {
            if (Track == null)
            {
                Debug.LogWarning($"Unassigned {nameof(Track)}, skipping this {nameof(TrackBackgroundPlayer)}");
                yield return Signal.BreakInto(Next);
                yield break;
            }

            context.RunAsynchronously(this, AsyncRunner(Track));

            yield return Signal.BreakInto(Next);
        }

        public override void FastForward(IContext context)
        {
            if (Track == null)
            {
                Debug.LogWarning($"Unassigned {nameof(Track)}, skipping this {nameof(TrackBackgroundPlayer)}");
                return;
            }

            context.RunAsynchronously(this, AsyncRunner(Track));
        }

        public override void SetupPreview(IPreviewer previewer, bool fastForwarded)
        {
            if (Track == null)
                return;

            foreach (var trackItem in Track.Items)
                trackItem?.AppendRollbackMechanism(previewer);

            previewer.RunAsynchronously(this, AsyncRunner(Track));
        }
    }
}
