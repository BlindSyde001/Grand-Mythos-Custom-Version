using UnityEngine;
using UnityEngine.UI;
using TMPro;

[AddComponentMenu(" GrandMythos/UIBinding/NewComponentContainer")]
public class NewComponentContainer : MonoBehaviour
{
    public required Button cmpButton;
    public required TextMeshProUGUI cmpName;

    public required ActionCondition selectedCnd;
    [SerializeReference] public required IAction selectedAction;
}