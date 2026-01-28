using UnityEngine;
using UnityEngine.UI;
using TMPro;

[AddComponentMenu(" GrandMythos/UIBinding/HeroPrefabUIData")]
public class HeroPrefabUIData : MonoBehaviour
{
    public required Image CharacterIcon;
    public required TextMeshProUGUI Health;
    public required Image AtbBar;
    public required Image ChargeBar;
    public required Image FlowBar;
    public required TextMeshProUGUI ManaLabel;
    public required TextMeshProUGUI NameLabel;
    public required RectTransform ModifierContainer;
    public required RectTransform ModifierContainer2;
    public required RectTransform Highlight;
}
