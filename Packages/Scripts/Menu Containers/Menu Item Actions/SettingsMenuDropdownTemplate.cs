using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Menu_Containers.Menu_Item_Actions
{
    public class SettingsMenuDropdownTemplate : MonoBehaviour
    {
        [Required] public TMP_Text Label;
        [FormerlySerializedAs("Drowpdown")] [Required] public TMP_Dropdown Dropdown;
    }
}