using System;

namespace YNode
{
    /// <summary> Prevents Node of the same type to be added more than once (configurable) to a NodeGraph </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DisallowMultipleNodesAttribute : Attribute
    {
        // TODO: Make inheritance work in such a way that applying [DisallowMultipleNodes(1)] to type NodeBar : Node
        //       while type NodeFoo : NodeBar exists, will let you add *either one* of these nodes, but not both.
        public int max;

        /// <summary> Prevents Node of the same type to be added more than once (configurable) to a NodeGraph </summary>
        /// <param name="max"> How many nodes to allow. Defaults to 1. </param>
        public DisallowMultipleNodesAttribute(int max = 1)
        {
            this.max = max;
        }
    }
}