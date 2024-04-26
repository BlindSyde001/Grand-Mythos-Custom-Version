using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class AbilitiesMenuActions : MenuContainer
{
    public UIElementList<SelectedHeroView> HeroSelectionUI = new();
    public UIElementList<AbilityButtonContainer> AbilityUI = new();
    [FormerlySerializedAs("abilityDescriptionContainer")] public AbilityDescriptionContainer AbilityDescriptionContainer;
    [Required] public InputActionReference SwitchHero;
    HeroExtension _selectedHero;

    // METHODS
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        SetupHeroSelectionUI();
        SetAbilities(GameManager.PartyLineup[0]);

        gameObject.SetActive(true);
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(500, 470, 0), menuInputs.Speed);

        foreach (var image in gameObject.transform.GetComponentsInChildren<Graphic>())
        {
            if (image.isActiveAndEnabled == false || image.gameObject.activeInHierarchy == false)
                continue;
            var col = image.color;
            col.a = 0;
            image.color = col;
            image.DOFade(1, menuInputs.Speed);
        }
        yield return new WaitForSeconds(menuInputs.Speed);
        SwitchHero.action.performed += Switch;
    }

    void Switch(InputAction.CallbackContext input)
    {
        int indexOf = GameManager.PartyLineup.IndexOf(_selectedHero);
        indexOf += input.ReadValue<float>() >= 0f ? 1 : -1;
        indexOf = indexOf < 0 ? GameManager.PartyLineup.Count + indexOf : indexOf % GameManager.PartyLineup.Count;

        SetAbilities(GameManager.PartyLineup[indexOf]);
    }

    public override IEnumerable Close(MenuInputs menuInputs)
    {
        SwitchHero.action.performed -= Switch;
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(500, 610, 0), menuInputs.Speed);
        foreach (var image in gameObject.transform.GetComponentsInChildren<Graphic>())
        {
            if (image.isActiveAndEnabled == false || image.gameObject.activeInHierarchy == false)
                continue;
            image.DOFade(0, menuInputs.Speed);
        }

        yield return new WaitForSeconds(menuInputs.Speed);
        gameObject.SetActive(false);
    }

    void SetupHeroSelectionUI()
    {
        AbilityDescriptionContainer.abilityNameTitle.text = "";
        AbilityDescriptionContainer.abilityDescription.text = "";
        HeroSelectionUI.Clear();
        foreach (var hero in GameManager.PartyLineup)
        {
            HeroSelectionUI.Allocate(out var selectedHeroView);
            selectedHeroView.GetComponent<Image>().sprite = hero.Portrait;
            selectedHeroView.Button.onClick.AddListener(() => SetAbilities(hero));
        }
    }

    void SetAbilities(HeroExtension hero)
    {
        _selectedHero = hero;

        AbilityDescriptionContainer.abilityNameTitle.text = "";
        AbilityDescriptionContainer.abilityDescription.text = "";
        AbilityUI.Clear();
        foreach (var skill in hero.Skills)
        {
            AbilityUI.Allocate(out var button);
            button.gameObject.SetActive(true);
            button.buttonName.text = skill.name;
            button.thisButton.onClick.AddListener(() => SetDescription(skill));
        }

        HighlightSelectedHero(HeroSelectionUI, _selectedHero);
    }

    void SetDescription(IAction skill)
    {
        AbilityDescriptionContainer.abilityNameTitle.text = skill.Name;
        AbilityDescriptionContainer.abilityDescription.text = skill.Description;
    }
}
