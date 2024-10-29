using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[AddComponentMenu(" GrandMythos/UIBinding/SaveFileButton")]
public class SaveFileButton : MonoBehaviour
{
    public TextMeshProUGUI areaName;
    public TextMeshProUGUI zoneName;
    public TextMeshProUGUI fileName;

    public TextMeshProUGUI timePlayed;
    public TextMeshProUGUI moneyAcquired;

    public List<Image> characterPortraits;
    [Required] public Button Button;
}
