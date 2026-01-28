using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public abstract class MenuContainer : SelectionTracker
{
    MenuInputs? _menuInputs;
    protected MenuInputs MenuInputs => _menuInputs ??= FindObjectOfType<MenuInputs>();
    protected GameManager GameManager => GameManager.Instance;
    protected InventoryManager InventoryManager => InventoryManager.Instance;

    public abstract IEnumerable Open(MenuInputs menuInputs);
    public abstract IEnumerable Close(MenuInputs menuInputs);
}

public abstract class MenuContainerWithHeroSelection : MenuContainer
{
    public UIElementList<SelectedHeroView> HeroSelectionUI = new(){ Template = null! };
    [Required, SerializeField] InputActionReference SwitchHero = null!;
    [ReadOnly, SerializeField] protected HeroExtension? SelectedHero;

    protected override void Update()
    {
        float dir = SwitchHero.action.ReadValue<float>();
        if (SwitchHero.action.WasPerformedThisFrameUnique())
        {
            int indexOf = GameManager.PartyLineup.IndexOf(SelectedHero!);
            indexOf += dir >= 0f ? 1 : -1;
            indexOf = indexOf < 0 ? GameManager.PartyLineup.Count + indexOf : indexOf % GameManager.PartyLineup.Count;

            ChangeSelectedHero(GameManager.PartyLineup[indexOf]);
        }

        base.Update();
    }

    public override IEnumerable Open(MenuInputs menuInputs)
    {
        HeroSelectionUI.Clear();
        foreach (var hero in GameManager.PartyLineup)
        {
            HeroSelectionUI.Allocate(out var element);
            element.GetComponent<Image>().sprite = hero.Portrait;
            element.Button.onClick.AddListener(() => ChangeSelectedHero(hero));
        }

        ChangeSelectedHero(GameManager.PartyLineup[0]);

        yield break;
    }

    void ChangeSelectedHero(HeroExtension hero)
    {
        SelectedHero = hero;
        foreach (var selectedHeroView in HeroSelectionUI)
        {
            var block = selectedHeroView.Button.colors;
            block.normalColor = Color.gray;
            selectedHeroView.Button.colors = block;
            selectedHeroView.Outline.enabled = false;
        }

        if (GameManager.PartyLineup.IndexOf(SelectedHero) is var indexOf and >= 0)
        {
            var ui = HeroSelectionUI[indexOf];
            var block = ui.Button.colors;
            block.normalColor = Color.white;
            ui.Button.colors = block;
            ui.Outline.enabled = true;
        }

        OnSelectedHeroChanged();
    }

    protected abstract void OnSelectedHeroChanged();
}