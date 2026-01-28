using UnityEngine;
using UnityEngine.UI;
using TMPro;

[AddComponentMenu(" GrandMythos/UIBinding/TacticsModuleContainer")]
public class TacticsModuleContainer : ActionSetContainer
{
    public required TextMeshProUGUI onToggle;
    public required Button onToggleBtn;

    public required TextMeshProUGUI condition;
    public required Button conditionBtn;
}
