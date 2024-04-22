using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class MenuContainer : SelectionTracker
{
    MenuInputs _menuInputs;
    protected MenuInputs MenuInputs => _menuInputs ??= FindObjectOfType<MenuInputs>();
    protected GameManager GameManager => GameManager.Instance;
    protected InventoryManager InventoryManager => InventoryManager.Instance;

    public abstract IEnumerable Open(MenuInputs menuInputs);
    public abstract IEnumerable Close(MenuInputs menuInputs);

    protected void HighlightSelectedHero(UIElementList<Button> HeroSelectionUI, HeroExtension hero)
    {
        foreach (var button in HeroSelectionUI)
        {
            var block = button.colors;
            block.normalColor = Color.gray;
            button.colors = block;
        }

        if (GameManager.PartyLineup.IndexOf(hero) is int indexOf and >= 0)
        {
            var block = HeroSelectionUI[indexOf].colors;
            block.normalColor = Color.white;
            HeroSelectionUI[indexOf].colors = block;
        }
    }
}