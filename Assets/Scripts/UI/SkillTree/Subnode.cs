using UnityEngine;
using UnityEngine.Events;

namespace UI.SkillTree
{
    public class Subnode : MonoBehaviour
    {
        public UnityEvent OnUnlock, OnLock, OnEnabled, OnDisabled;
    }
}