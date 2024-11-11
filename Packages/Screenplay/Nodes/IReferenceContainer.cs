using System.Collections.Generic;
using YNode;

namespace Screenplay.Nodes
{
    /// <summary>
    /// This node contains cross-scene references
    /// </summary>
    public interface IReferenceContainer : INodeValue
    {
        /// <summary>
        /// Appends this node's cross-scene references to the list.
        /// </summary>
        /// <remarks>
        /// Used to mark objects that are referenced by a <see cref="ScreenplayGraph"/> in the scene.
        /// </remarks>
        void CollectReferences(List<GenericSceneObjectReference> references);
    }
}
