using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AbilitiesMenuActions : MenuContainer
{
    public UIElementList<Button> HeroSelectionUI = new();
    public UIElementList<AbilityButtonContainer> AbilityUI = new();
    public AbilityDescriptionContainer abilityDescriptionContainer;
    HeroExtension selectedHero;

    // METHODS
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        gameObject.SetActive(true);
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(500, 470, 0), menuInputs.Speed);

        SetHeroSelection();
        SetAbilities(GameManager.PartyLineup[0]);

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
    }
    public override IEnumerable Close(MenuInputs menuInputs)
    {
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-1200, 480, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(500, 610, 0), menuInputs.Speed);
        foreach (var image in gameObject.transform.GetComponentsInChildren<Graphic>())
            image.DOFade(0, menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
        gameObject.SetActive(false);
    }

    internal void SetHeroSelection()
    {
        abilityDescriptionContainer.abilityNameTitle.text = "";
        abilityDescriptionContainer.abilityDescription.text = "";
        HeroSelectionUI.Clear();
        foreach (var hero in GameManager.PartyLineup)
        {
            HeroSelectionUI.Allocate(out var button);
            button.GetComponent<Image>().sprite = hero.Portrait;
            button.onClick.AddListener(() => SetAbilities(hero));
        }
    }

    void SetAbilities(HeroExtension hero)
    {
        selectedHero = hero;

        abilityDescriptionContainer.abilityNameTitle.text = "";
        abilityDescriptionContainer.abilityDescription.text = "";
        AbilityUI.Clear();
        foreach (var skill in hero.Skills)
        {
            AbilityUI.Allocate(out var button);
            button.gameObject.SetActive(true);
            button.buttonName.text = skill.name;
            button.thisButton.onClick.AddListener(() => SetDescription(skill));
        }
    }

    void SetDescription(IAction skill)
    {
        abilityDescriptionContainer.abilityNameTitle.text = skill.Name;
        abilityDescriptionContainer.abilityDescription.text = skill.Description;
    }
}
