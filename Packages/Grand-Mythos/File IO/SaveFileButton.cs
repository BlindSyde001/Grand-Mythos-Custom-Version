using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[AddComponentMenu(" GrandMythos/UIBinding/SaveFileButton")]
public class SaveFileButton : MonoBehaviour
{
    public required TextMeshProUGUI areaName;
    public required TextMeshProUGUI zoneName;
    public required TextMeshProUGUI fileName;

    public required TextMeshProUGUI timePlayed;
    public required TextMeshProUGUI moneyAcquired;

    public List<Image> characterPortraits = new();
    public required Button Button;
}
