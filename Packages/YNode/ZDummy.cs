using UnityEngine;

namespace YNode
{
    internal class ZDummy : ScriptableObject
    {
        [SerializeReference]
        public INodeValue? Value;
    }
}
