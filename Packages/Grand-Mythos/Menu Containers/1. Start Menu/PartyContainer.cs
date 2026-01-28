using UnityEngine;
using UnityEngine.UI;
using TMPro;

[AddComponentMenu(" GrandMythos/UIBinding/PartyContainer")]
public class PartyContainer : MonoBehaviour
{
    public required TextMeshProUGUI displayName;
    public required Image displayBanner;
    public required TextMeshProUGUI displayLevel;
    public required Image displayEXPBar;
    public required TextMeshProUGUI displayHP;
    public required TextMeshProUGUI displayEXPToNextLevel;
    public required Button ChangeOrderButton;
}
