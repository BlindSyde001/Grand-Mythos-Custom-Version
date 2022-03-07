using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Consumables")]
public class Consumable : BaseItem
{
    public Action myAction;
}