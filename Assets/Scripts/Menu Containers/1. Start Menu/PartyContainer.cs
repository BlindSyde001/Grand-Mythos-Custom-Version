using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[AddComponentMenu(" GrandMythos/UIBinding/PartyContainer")]
public class PartyContainer : MonoBehaviour
{
    [Required] public TextMeshProUGUI displayName;
    [Required] public Image displayBanner;
    [Required] public TextMeshProUGUI displayLevel;
    [Required] public Image displayEXPBar;
    [Required] public TextMeshProUGUI displayHP;
    [Required] public TextMeshProUGUI displayEXPToNextLevel;
    [Required] public Button ChangeOrderButton;
}
