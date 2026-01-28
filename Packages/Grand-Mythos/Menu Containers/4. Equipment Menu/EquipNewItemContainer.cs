using UnityEngine;
using UnityEngine.UI;
using TMPro;

[AddComponentMenu(" GrandMythos/UIBinding/EquipNewItemContainer")]
public class EquipNewItemContainer : MonoBehaviour
{
    public required TextMeshProUGUI EquipName;
    public required Equipment? ThisEquipment;
    public required Button ThisButton;
}
