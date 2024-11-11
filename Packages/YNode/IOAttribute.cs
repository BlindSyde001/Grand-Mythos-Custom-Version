using System;

namespace YNode
{
    public abstract class IOAttribute : Attribute
    {
        public NoodleStroke Stroke = NoodleStroke.Full;
    }
}
