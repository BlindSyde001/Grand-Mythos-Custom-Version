using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu(" GrandMythos/UIBinding/ActionSetContainer")]
public class ActionSetContainer : MonoBehaviour
{
    public List<Button> addActionBtns = new();

    public List<Button> singleActionBtns = new();
    public List<Button> doubleActionBtns = new();
    public List<Button> tripleActionBtns = new();
    public required Button quadActionBtn;
    public List<TextMeshProUGUI> singlesText = new();
    public List<TextMeshProUGUI> doublesText = new();
    public List<TextMeshProUGUI> triplesText = new();
    public required TextMeshProUGUI quadruplesText;
}