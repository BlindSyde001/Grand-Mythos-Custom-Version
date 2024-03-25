using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[AddComponentMenu(" GrandMythos/UIBinding/TacticsModuleContainer")]
public class TacticsModuleContainer : MonoBehaviour
{
    public TextMeshProUGUI onToggle;
    public Button onToggleBtn;

    public TextMeshProUGUI condition;
    public Button conditionBtn;

    public List<Button> addActionBtns;

    public List<Button> singleActionBtns;
    public List<Button> doubleActionBtns;
    public List<Button> tripleActionBtns;
    public Button quadActionBtn;
    public List<TextMeshProUGUI> singlesText;
    public List<TextMeshProUGUI> doublesText;
    public List<TextMeshProUGUI> triplesText;
    public TextMeshProUGUI quadruplesText;
}
