using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu(" GrandMythos/UIBinding/ShopItemButton")]
public class ShopItemButton : MonoBehaviour
{
    [Required] public Button Button;
    [Required] public TMP_Text Label;
    [Required] public TMP_Text Cost;
}