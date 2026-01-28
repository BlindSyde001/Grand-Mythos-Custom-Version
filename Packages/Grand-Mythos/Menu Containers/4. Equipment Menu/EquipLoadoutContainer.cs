using UnityEngine;
using UnityEngine.UI;
using TMPro;

[AddComponentMenu(" GrandMythos/UIBinding/EquipLoadoutContainer")]
public class EquipLoadoutContainer : MonoBehaviour
{
    public required TextMeshProUGUI EquippedName;
    public required Equipment? thisEquipment;
    public required Button ThisButton;
}
