using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class AbilitiesMenuActions : MonoBehaviour
{

    // VARIABLES
    [SerializeField]
    private GameManager gameManager;
    private InputManager inputManager;
    private MenuInputs menuInputs;

    public List<Button> heroSelections;
    public List<AbilityButtonContainer> abilityButtons;
    public AbilityDescriptionContainer abilityDescriptionContainer;

    private HeroExtension selectedHero;
    // UPDATES
    private void Start()
    {
        gameManager = GameManager._instance;
        inputManager = InputManager._instance;
        menuInputs = FindObjectOfType<MenuInputs>();
    }

    // METHODS
    internal IEnumerator AbilitiesMenuOpen()
    {
        if (!menuInputs.coroutineRunning)
        {
            yield return new WaitForSeconds(menuInputs.speed);
            inputManager.MenuItems[2].SetActive(true);
            inputManager.MenuItems[2].transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.speed);
            inputManager.MenuItems[2].transform.GetChild(1).DOLocalMove(new Vector3(500, 470, 0), menuInputs.speed);
            inputManager.MenuItems[2].transform.GetChild(2).GetComponent<Image>().DOFillAmount(1, menuInputs.speed);
            foreach(AbilityButtonContainer a in abilityButtons)
            {
                a.thisButton.GetComponent<Image>().DOFade(1, menuInputs.speed);
                a.buttonName.DOFade(1, menuInputs.speed);
            }
            SetHeroSelection();
            SetAbilities(gameManager._PartyLineup[0]);
        }
    }
    internal IEnumerator AbilitiesMenuClose(bool closeAllOverride)
    {
        if (!menuInputs.coroutineRunning)
        {
            menuInputs.coroutineRunning = true;
            inputManager.MenuItems[2].transform.GetChild(0).DOLocalMove(new Vector3(-1200, 480, 0), menuInputs.speed);
            inputManager.MenuItems[2].transform.GetChild(1).DOLocalMove(new Vector3(500, 610, 0), menuInputs.speed);
            inputManager.MenuItems[2].transform.GetChild(2).GetComponent<Image>().DOFillAmount(0, menuInputs.speed);
            foreach (AbilityButtonContainer a in abilityButtons)
            {
                a.thisButton.GetComponent<Image>().DOFade(0, .5f * menuInputs.speed);
                a.buttonName.DOFade(0, .5f * menuInputs.speed);
            }
            yield return new WaitForSeconds(menuInputs.speed);
            inputManager.MenuItems[2].SetActive(false);
            menuInputs.coroutineRunning = false;
        }
        if (!closeAllOverride)
        {
            menuInputs.startMenuActions.StartMenuOpen();
            yield return new WaitForSeconds(menuInputs.speed);
            menuInputs.currentMenuOpen = 0;
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
        for (int i = 0; i < gameManager._PartyLineup.Count; i++)
        {
            int j = i;
            heroSelections[i].gameObject.SetActive(true);
            heroSelections[i].onClick.AddListener(delegate { SetAbilities(gameManager._PartyLineup[j]); });
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
        for(int i = 0; i < hero._AvailableActions.Count; i++)
        {
            int j = i;
            abilityButtons[i].gameObject.SetActive(true);
            abilityButtons[i].buttonName.text = hero._AvailableActions[i]._Name;
            abilityButtons[i].thisButton.onClick.AddListener(delegate { SetDescription(hero._AvailableActions[j]); });
        }
    }
    public void SetDescription(Action action)
    {
        abilityDescriptionContainer.abilityNameTitle.text = action._Name;
        abilityDescriptionContainer.abilityDescription.text = action._Description;
    }
}
