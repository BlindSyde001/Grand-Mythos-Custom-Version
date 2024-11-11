namespace YNode.Editor
{
    public abstract class CustomNodeEditor<T> : NodeEditor where T : INodeValue
    {
        public new T Value => (T)base.Value;
    }
}
