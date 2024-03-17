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
        gameObject.transform.GetChild(2).GetComponent<Image>().DOFillAmount(1, menuInputs.Speed);
        foreach(AbilityButtonContainer a in AbilityUI)
        {
            a.thisButton.targetGraphic.DOFade(1, menuInputs.Speed);
            a.buttonName.DOFade(1, menuInputs.Speed);
        }
        SetHeroSelection();
        SetAbilities(GameManager.PartyLineup[0]);
        yield return new WaitForSeconds(menuInputs.Speed);
    }
    public override IEnumerable Close(MenuInputs menuInputs)
    {
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-1200, 480, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(500, 610, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).GetComponent<Image>().DOFillAmount(0, menuInputs.Speed);
        foreach (AbilityButtonContainer a in AbilityUI)
        {
            a.thisButton.targetGraphic.DOFade(0, .5f * menuInputs.Speed);
            a.buttonName.DOFade(0, .5f * menuInputs.Speed);
        }
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
            button.onClick.AddListener(delegate { SetAbilities(hero); });
        }
    }
    public void SetAbilities(HeroExtension hero)
    {
        selectedHero = hero;

        abilityDescriptionContainer.abilityNameTitle.text = "";
        abilityDescriptionContainer.abilityDescription.text = "";
        AbilityUI.Clear();
    }
}
