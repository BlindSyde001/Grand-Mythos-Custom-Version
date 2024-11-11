using System;

namespace YNode
{
    /// <summary> Mark a serializable field as an output port. You can access this through <see cref="Node.GetPort(string)" /> </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class OutputAttribute : IOAttribute
    {
        /// <summary> Mark a serializable field as an output port. You can access this through <see cref="Node.GetPort(string)" /> </summary>
        public OutputAttribute() { }
    }
}