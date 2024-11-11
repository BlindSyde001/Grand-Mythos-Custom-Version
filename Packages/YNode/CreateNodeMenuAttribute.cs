using System;

namespace YNode
{
    /// <summary> Manually supply node class with a context menu path </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CreateNodeMenuAttribute : Attribute
    {
        public string menuName;
        public int order;

        /// <summary> Manually supply node class with a context menu path </summary>
        /// <param name="menuName"> Path to this node in the context menu. Null or empty hides it. </param>
        public CreateNodeMenuAttribute(string menuName)
        {
            this.menuName = menuName;
            order = 0;
        }

        /// <summary> Manually supply node class with a context menu path </summary>
        /// <param name="menuName"> Path to this node in the context menu. Null or empty hides it. </param>
        /// <param name="order"> The order by which the menu items are displayed. </param>
        public CreateNodeMenuAttribute(string menuName, int order)
        {
            this.menuName = menuName;
            this.order = order;
        }
    }
}