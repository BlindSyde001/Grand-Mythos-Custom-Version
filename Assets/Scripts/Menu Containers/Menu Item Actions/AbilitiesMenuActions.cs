using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class AbilitiesMenuActions : MenuContainer
{
    public List<Button> heroSelections;
    public List<AbilityButtonContainer> abilityButtons;
    public AbilityDescriptionContainer abilityDescriptionContainer;

    private HeroExtension selectedHero;

    // METHODS
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        if (!menuInputs.coroutineRunning)
        {
            yield return new WaitForSeconds(menuInputs.speed);
            gameObject.SetActive(true);
            gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.speed);
            gameObject.transform.GetChild(1).DOLocalMove(new Vector3(500, 470, 0), menuInputs.speed);
            gameObject.transform.GetChild(2).GetComponent<Image>().DOFillAmount(1, menuInputs.speed);
            foreach(AbilityButtonContainer a in abilityButtons)
            {
                a.thisButton.GetComponent<Image>().DOFade(1, menuInputs.speed);
                a.buttonName.DOFade(1, menuInputs.speed);
            }
            SetHeroSelection();
            SetAbilities(GameManager._PartyLineup[0]);
        }
    }
    public override IEnumerable Close(MenuInputs menuInputs)
    {
        if (!menuInputs.coroutineRunning)
        {
            menuInputs.coroutineRunning = true;
            gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-1200, 480, 0), menuInputs.speed);
            gameObject.transform.GetChild(1).DOLocalMove(new Vector3(500, 610, 0), menuInputs.speed);
            gameObject.transform.GetChild(2).GetComponent<Image>().DOFillAmount(0, menuInputs.speed);
            foreach (AbilityButtonContainer a in abilityButtons)
            {
                a.thisButton.GetComponent<Image>().DOFade(0, .5f * menuInputs.speed);
                a.buttonName.DOFade(0, .5f * menuInputs.speed);
            }
            yield return new WaitForSeconds(menuInputs.speed);
            gameObject.SetActive(false);
            menuInputs.coroutineRunning = false;
        }
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
        for (int i = 0; i < GameManager._PartyLineup.Count; i++)
        {
            int j = i;
            heroSelections[i].gameObject.SetActive(true);
            heroSelections[i].GetComponent<Image>().sprite = GameManager._PartyLineup[j].charPortrait;
            heroSelections[i].onClick.AddListener(delegate { SetAbilities(GameManager._PartyLineup[j]); });
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
