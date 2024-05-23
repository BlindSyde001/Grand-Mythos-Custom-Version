using Sirenix.OdinInspector;
using UnityEngine;

public class TradeableItem : BaseItem
{
    public int Cost = 10;

    [InfoBox("What happens when the player sells this item")]
    [SerializeReference]
    public IInteraction OnPlayerSoldItem;
}