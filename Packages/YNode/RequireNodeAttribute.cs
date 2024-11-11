using System;

namespace YNode
{
    /// <summary> Automatically ensures the existance of a certain node type, and prevents it from being deleted. </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequireNodeAttribute : Attribute
    {
        public Type? type0;
        public Type? type1;
        public Type? type2;

        /// <summary> Automatically ensures the existance of a certain node type, and prevents it from being deleted </summary>
        public RequireNodeAttribute(Type type)
        {
            type0 = type;
            type1 = null;
            type2 = null;
        }

        /// <summary> Automatically ensures the existance of a certain node type, and prevents it from being deleted </summary>
        public RequireNodeAttribute(Type type, Type type2)
        {
            type0 = type;
            type1 = type2;
            this.type2 = null;
        }

        /// <summary> Automatically ensures the existance of a certain node type, and prevents it from being deleted </summary>
        public RequireNodeAttribute(Type type, Type type2, Type type3)
        {
            type0 = type;
            type1 = type2;
            this.type2 = type3;
        }

        public bool Requires(Type? type)
        {
            if (type == null)
                return false;
            if (type == type0)
                return true;
            if (type == type1)
                return true;
            if (type == type2)
                return true;
            return false;
        }
    }
}
