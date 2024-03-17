using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[AddComponentMenu(" GrandMythos/UIBinding/AbilityButtonContainer")]
public class AbilityButtonContainer : MonoBehaviour
{
    [Required] public Button thisButton;
    [Required] public TextMeshProUGUI buttonName;
}
