using UnityEngine;
using UnityEngine.UI;
using TMPro;

[AddComponentMenu(" GrandMythos/UIBinding/NewComponentContainer")]
public class NewComponentContainer : MonoBehaviour
{
    public Button cmpButton;
    public TextMeshProUGUI cmpName;

    public ActionCondition selectedCnd;
    [SerializeReference] public IAction selectedAction;
}