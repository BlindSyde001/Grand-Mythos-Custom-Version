using YNode;

namespace Screenplay.Nodes
{
    /// <summary>
    /// A node that can be executed in the editor as a preview
    /// </summary>
    public interface IPreviewable : INodeValue
    {
        /// <summary>
        /// Called when a preview containing this node is created, implementer use the <paramref name="previewer"/> provided
        /// to enqueue their custom preview logic and rollback mechanism to deal with any side effects that preview incurs on the scene.
        /// </summary>
        /// <param name="previewer">The container to run previews and ensure scene changes can be rolled back</param>
        /// <param name="fastForwarded">Whether the node should be played or fast forwarded, see <see cref="IAction.FastForward"/></param>
         void SetupPreview(IPreviewer previewer, bool fastForwarded);
    }
}
