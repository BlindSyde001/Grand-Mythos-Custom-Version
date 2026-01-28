using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

[AddComponentMenu(" GrandMythos/UIBinding/ItemButtonContainer")]
public class ItemButtonContainer : MonoBehaviour
{
    [FormerlySerializedAs("itemName")] public required TextMeshProUGUI ItemName;
    [FormerlySerializedAs("itemAmount")] public required TextMeshProUGUI ItemAmount;
    public required Button Button;
    public string itemDescription = "";
}
