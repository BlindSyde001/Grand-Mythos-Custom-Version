using System;

namespace YNode
{
    /// <summary> Specify a width for this node type </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeWidthAttribute : Attribute
    {
        public const int Default = 208;
        public int width;

        /// <summary> Specify a width for this node type </summary>
        /// <param name="width"> Width </param>
        public NodeWidthAttribute(int width)
        {
            this.width = width;
        }
    }
}
