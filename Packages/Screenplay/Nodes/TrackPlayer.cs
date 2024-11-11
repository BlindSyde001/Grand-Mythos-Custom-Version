using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YNode;

namespace Screenplay.Nodes
{
    public class TrackPlayer : Action
    {
        [SerializeReference, Input, Required] public Track? Track;
        [SerializeField, HideInInspector] public int From = -1, To = -1;

        public override void CollectReferences(List<GenericSceneObjectReference> references) => Track?.CollectReferences(references);

        public override IEnumerable<Signal> Execute(IContext context)
        {
            if (Track == null)
            {
                Debug.LogWarning($"Unassigned {nameof(Track)}, skipping this {nameof(TrackPlayer)}");
                yield return Signal.BreakInto(Next);
                yield break;
            }

            foreach (var signal in Track.RangePlayer(GetTimeSpan(Track), false))
                yield return signal;

            yield return Signal.BreakInto(Next);
        }

        public override void FastForward(IContext context)
        {
            if (Track == null)
            {
                Debug.LogWarning($"Unassigned {nameof(Track)}, skipping this {nameof(TrackPlayer)}");
                return;
            }

            var timespan = GetTimeSpan(Track);
            using var samplers = Track.GetDisposableSamplers();
            foreach (var sampler in samplers)
            {
                if (timespan.end >= sampler.start)
                    sampler.sampler.Sample(timespan.start, timespan.end);
            }
        }

        public override void SetupPreview(IPreviewer previewer, bool fastForwarded)
        {
            if (Track == null)
                return;

            foreach (var trackItem in Track.Items)
                trackItem?.AppendRollbackMechanism(previewer);

            if (fastForwarded)
                FastForward(previewer);
            else
                previewer.PlaySafeAction(this);
        }

        protected (float start, float end) GetTimeSpan(Track track) => (GetMarker(true, From, track), GetMarker(false, To, track));

        private static float GetMarker(bool start, int id, Track track)
        {
            if (id == -1)
                return start ? 0f : track.Duration();

            if (id < 0 || id >= track.Markers.Length)
            {
                Debug.LogWarning($"TrackPlayer marker for {(start ? "start" : "end")} has unknown id '{id}' set, returning default range");
                return start ? 0f : track.Duration();
            }

            return track.Markers[id].Time;
        }
    }
}
