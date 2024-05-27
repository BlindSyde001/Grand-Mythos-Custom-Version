using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu(" GrandMythos/UIBinding/ActionSetContainer")]
public class ActionSetContainer : MonoBehaviour
{
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