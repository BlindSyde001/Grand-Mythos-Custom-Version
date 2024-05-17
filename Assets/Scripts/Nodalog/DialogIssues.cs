#nullable enable

using System;
using System.Collections.Generic;

namespace Nodalog
{
    [Serializable]
    public class DialogIssues
    {
        [NonSerialized]
        public List<Message> Issues = new();

        [Serializable]
        public struct Message
        {
            public string Text;
            public Type Type;
        }

        public enum Type
        {
            None,
            Info,
            Warning,
            Error,
        }
    }
}