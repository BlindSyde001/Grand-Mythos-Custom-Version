using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[AddComponentMenu(" GrandMythos/UIBinding/HeroPrefabUIData")]
public class HeroPrefabUIData : MonoBehaviour
{
    [Required] public Image CharacterIcon;
    [Required] public TextMeshProUGUI Health;
    [Required] public Image AtbBar;
    [Required] public Image ChargeBar;
    [Required] public Image FlowBar;
    [Required] public TextMeshProUGUI ManaLabel;
    [Required] public TextMeshProUGUI NameLabel;
    [Required] public RectTransform ModifierContainer;
    [Required] public RectTransform ModifierContainer2;
    [Required] public RectTransform Highlight;
}
