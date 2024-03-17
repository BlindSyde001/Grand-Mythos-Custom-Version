using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

[AddComponentMenu(" GrandMythos/UIBinding/ItemButtonContainer")]
public class ItemButtonContainer : MonoBehaviour
{
    [FormerlySerializedAs("itemName"),Required] public TextMeshProUGUI ItemName;
    [FormerlySerializedAs("itemAmount"),Required] public TextMeshProUGUI ItemAmount;
    [Required] public Button Button;
    public string itemDescription;
}
