using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace QTE
{
    [CreateAssetMenu]
    public class QTEStart : ScriptableObject
    {
        [Required] public InputActionReference Input;
        public EventType EventType = EventType.PlayerAttack;
        [Required] public QTEInterface Interface;
        [SerializeReference, Required] public IQTE QTEType;

        public Skill Counter;
    }

    public enum EventType
    {
        PlayerAttack,
        PlayerDodge,
        PlayerParry,
        PlayerShield
    }
}