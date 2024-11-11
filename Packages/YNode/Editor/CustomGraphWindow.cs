namespace YNode.Editor
{
    public abstract class CustomGraphWindow<T> : GraphWindow where T : NodeGraph
    {
        public new T Graph => (T)base.Graph;

        public override string PreferenceKey => typeof(T).FullName!;
    }
}