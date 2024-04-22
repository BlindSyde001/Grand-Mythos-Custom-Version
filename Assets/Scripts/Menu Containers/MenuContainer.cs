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

    protected void HighlightSelectedHero(UIElementList<SelectedHeroView> HeroSelectionUI, HeroExtension hero)
    {
        foreach (var selectedHeroView in HeroSelectionUI)
        {
            var block = selectedHeroView.Button.colors;
            block.normalColor = Color.gray;
            selectedHeroView.Button.colors = block;
            selectedHeroView.Outline.enabled = false;
        }

        if (GameManager.PartyLineup.IndexOf(hero) is int indexOf and >= 0)
        {
            var ui = HeroSelectionUI[indexOf];
            var block = ui.Button.colors;
            block.normalColor = Color.white;
            ui.Button.colors = block;
            ui.Outline.enabled = true;
        }
    }
}