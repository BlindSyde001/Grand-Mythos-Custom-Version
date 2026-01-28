using Sirenix.OdinInspector;
using UnityEngine;

public class TradeableItem : BaseItem
{
    public int Cost = 10;

    [Tooltip("What happens when the player sells this item"), SerializeReference, ValidateInput(nameof(ValidateOnPlayerSoldItem))]
    public IInteraction? OnPlayerSoldItem;

    static bool ValidateOnPlayerSoldItem(IInteraction? interaction, ref string? message)
    {
        return interaction == null || interaction.IsValid(out message);
    }
}