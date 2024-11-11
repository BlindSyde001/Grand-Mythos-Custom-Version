using System;
using System.Collections.Generic;
using Screenplay.Nodes.TrackItems;
using Sirenix.OdinInspector;
using UnityEngine;
using YNode;

namespace Screenplay.Nodes
{
    [Serializable, NodeWidth(800), NodeTint(0.25f, 0.25f, 0.25f)]
    public class Track : ScreenplayNode, IPreviewable, INodeWithSceneGizmos
    {
        [SerializeReference, InlineProperty]
        public ITrackItem?[] Items = Array.Empty<ITrackItem?>();
        public Marker[] Markers = Array.Empty<Marker>();

        [NonSerialized]
        public float DebugPlayHead;
        [NonSerialized]
        public PreviewMode DebugScrub = PreviewMode.Scrub;

        public float Duration()
        {
            float duration = 0;
            foreach (var trackItem in Items)
            {
                if (trackItem is not null && trackItem.Timespan.end > duration)
                    duration = trackItem.Timespan.end;
            }

            return duration;
        }

        public IEnumerable<Signal> RangePlayer((float start, float end) timespan, bool loop)
        {
            using var samplers = GetDisposableSamplers();
            float t = timespan.start;
            float previousT = t;
            do
            {
                t += Time.deltaTime;
                if (loop && t >= timespan.end)
                     t -= timespan.end - timespan.start;

                foreach (var sampler in samplers)
                {
                    if (t >= sampler.start)
                        sampler.sampler.Sample(previousT, t);
                }

                previousT = t;
                yield return Signal.NextFrame;
            } while (loop || t < timespan.end);
        }

        public override void CollectReferences(List<GenericSceneObjectReference> references)
        {
            foreach (var trackItem in Items)
                trackItem?.CollectReferences(references);
        }

        public void SetupPreview(IPreviewer previewer, bool fastForwarded)
        {
            foreach (var trackItem in Items)
                trackItem?.AppendRollbackMechanism(previewer);

            previewer.PlayCustomSignal(Preview());
            return;

            IEnumerable<Signal> Preview()
            {
                var timespan = (start:0f, end:Duration());
                using var samplers = GetDisposableSamplers();

                float previousT = DebugPlayHead;
                do
                {
                    if (DebugScrub != PreviewMode.Scrub)
                        DebugPlayHead += Time.deltaTime;

                    foreach (var sampler in samplers)
                    {
                        if (DebugPlayHead >= sampler.start)
                            sampler.sampler.Sample(previousT, DebugPlayHead);
                    }

                    previousT = DebugPlayHead;
                    yield return Signal.NextFrame;
                    if (DebugScrub != PreviewMode.Scrub && previewer.Loop && DebugPlayHead >= timespan.end)
                        DebugPlayHead -= timespan.end;

                } while (DebugScrub == PreviewMode.Scrub || DebugPlayHead < timespan.end);
                // ReSharper disable once IteratorNeverReturns
            }
        }

        public SamplersList GetDisposableSamplers()
        {
            var samplers = new SamplersList(Items.Length);
            try
            {
                foreach (var trackItem in Items)
                {
                    var sampler = trackItem?.TryGetSampler();
                    if (sampler != null)
                        samplers.Add((sampler, trackItem!.Timespan.start));
                }
            }
            catch
            {
                foreach (var sampler in samplers)
                    sampler.sampler.Dispose();
            }

            return samplers;
        }

        public class SamplersList : List<(ITrackSampler sampler, float start)>, IDisposable
        {
            public SamplersList(int capacity) : base(capacity) { }

            public void Dispose()
            {
                foreach (var sampler in this)
                    sampler.sampler.Dispose();
            }
        }

        [Serializable]
        public struct Marker
        {
            [HorizontalGroup] public string? Name;
            [HorizontalGroup] public float Time;
        }

        public enum PreviewMode
        {
            Scrub,
            Play
        }

        public void DrawGizmos(ref bool rebuildPreview)
        {
            foreach (var item in Items)
            {
                if (item is TransformTrackItem tti)
                    tti.DrawGizmos(ref rebuildPreview);
            }
        }
    }
}
