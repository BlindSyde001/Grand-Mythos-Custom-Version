using System;

namespace Screenplay.Nodes.TrackItems
{
    public interface ITrackSampler : IDisposable
    {
        public void Sample(float previousTime, float t);
    }
}
