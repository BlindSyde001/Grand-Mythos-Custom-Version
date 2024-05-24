using UnityEngine;

public class TradeableItem : BaseItem
{
    public int Cost = 10;

    [Tooltip("What happens when the player sells this item"), SerializeReference]
    public IInteraction OnPlayerSoldItem;
}