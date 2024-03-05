using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AbilitiesMenuActions : MenuContainer
{
    public List<Button> heroSelections;
    public List<AbilityButtonContainer> abilityButtons;
    public AbilityDescriptionContainer abilityDescriptionContainer;
    HeroExtension selectedHero;

    // METHODS
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        gameObject.SetActive(true);
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(500, 470, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).GetComponent<Image>().DOFillAmount(1, menuInputs.Speed);
        foreach(AbilityButtonContainer a in abilityButtons)
        {
            a.thisButton.GetComponent<Image>().DOFade(1, menuInputs.Speed);
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
        foreach (AbilityButtonContainer a in abilityButtons)
        {
            a.thisButton.GetComponent<Image>().DOFade(0, .5f * menuInputs.Speed);
            a.buttonName.DOFade(0, .5f * menuInputs.Speed);
        }
        yield return new WaitForSeconds(menuInputs.Speed);
        gameObject.SetActive(false);
    }

    internal void SetHeroSelection()
    {
        abilityDescriptionContainer.abilityNameTitle.text = "";
        abilityDescriptionContainer.abilityDescription.text = "";
        foreach (Button a in heroSelections)
        {
            a.gameObject.SetActive(false);
            a.onClick.RemoveAllListeners();
        }
        for (int i = 0; i < GameManager.PartyLineup.Count; i++)
        {
            int j = i;
            heroSelections[i].gameObject.SetActive(true);
            heroSelections[i].GetComponent<Image>().sprite = GameManager.PartyLineup[j].Portrait;
            heroSelections[i].onClick.AddListener(delegate { SetAbilities(GameManager.PartyLineup[j]); });
        }
    }
    public void SetAbilities(HeroExtension hero)
    {
        selectedHero = hero;

        abilityDescriptionContainer.abilityNameTitle.text = "";
        abilityDescriptionContainer.abilityDescription.text = "";
        foreach (AbilityButtonContainer a in abilityButtons)
        {
            a.gameObject.SetActive(false);
        }
    }
}
