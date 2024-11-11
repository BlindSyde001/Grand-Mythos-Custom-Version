using YNode;

namespace Screenplay.Nodes
{
    public interface INodeWithSceneGizmos : INodeValue
    {
        void DrawGizmos(ref bool rebuildPreview);
    }
}
