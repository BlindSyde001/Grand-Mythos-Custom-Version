using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AbilitiesMenuActions : MenuContainerWithHeroSelection
{
    public UIElementList<AbilityButtonContainer> AbilityUI = new();
    public AbilityDescriptionContainer AbilityDescriptionContainer;

    // METHODS
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        foreach (var yields in base.Open(menuInputs))
        {
            yield return yields;
        }

        AbilityDescriptionContainer.abilityNameTitle.text = "";
        AbilityDescriptionContainer.abilityDescription.text = "";

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
    }

    public override IEnumerable Close(MenuInputs menuInputs)
    {
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

    protected override void OnSelectedHeroChanged()
    {
        AbilityDescriptionContainer.abilityNameTitle.text = "";
        AbilityDescriptionContainer.abilityDescription.text = "";
        AbilityUI.Clear();
        foreach (var skill in SelectedHero.Skills)
        {
            AbilityUI.Allocate(out var button);
            button.gameObject.SetActive(true);
            button.buttonName.text = skill.name;
            button.thisButton.onClick.AddListener(() => SetDescription(skill));
        }
    }

    void SetDescription(IAction skill)
    {
        AbilityDescriptionContainer.abilityNameTitle.text = skill.Name;
        AbilityDescriptionContainer.abilityDescription.text = skill.Description;
    }
}
