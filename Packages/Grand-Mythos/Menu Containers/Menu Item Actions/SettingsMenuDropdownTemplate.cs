using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Menu_Containers.Menu_Item_Actions
{
    public class SettingsMenuDropdownTemplate : MonoBehaviour
    {
        public required TMP_Text Label;
        [FormerlySerializedAs("Drowpdown")] public required TMP_Dropdown Dropdown;
    }
}