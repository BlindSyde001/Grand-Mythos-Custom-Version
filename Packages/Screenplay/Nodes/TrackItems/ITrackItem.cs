using System.Collections.Generic;

namespace Screenplay.Nodes.TrackItems
{
    public interface ITrackItem
    {
        string Label { get; }
        float Start { get; set; }
        float Duration { get; set; }
        (float start, float end) Timespan => (Start, Start + Duration);
        void CollectReferences(List<GenericSceneObjectReference> references);
        ITrackSampler? TryGetSampler();
        void AppendRollbackMechanism(IPreviewer previewer);
    }
}
