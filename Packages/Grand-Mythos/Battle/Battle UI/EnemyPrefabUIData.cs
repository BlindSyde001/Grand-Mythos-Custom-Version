using UnityEngine;
using UnityEngine.UI;
using TMPro;

[AddComponentMenu(" GrandMythos/UIBinding/EnemyPrefabUIData")]
public class EnemyPrefabUIData : MonoBehaviour
{
    public required TextMeshProUGUI identity;
    public required TextMeshProUGUI health;
    public required Image healthBar;
}
