using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

[AddComponentMenu(" GrandMythos/UIBinding/HeroPrefabUIData")]
public class HeroPrefabUIData : MonoBehaviour
{
    [Required][FormerlySerializedAs("characterIcon")] public Image CharacterIcon;
    [Required][FormerlySerializedAs("health")] public TextMeshProUGUI Health;
    [Required][FormerlySerializedAs("atbBar")] public Image AtbBar;
    [Required][FormerlySerializedAs("healthBar")] public Image HealthBar;
    [Required][FormerlySerializedAs("nameLabel")] public TextMeshProUGUI NameLabel;
    [Required]public Image BorderImage;
}
