using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class MenuInputs : MonoBehaviour
{
    // VARIABLES
    private InputManager inputManager;
    private GameManager gameManager;
    private InventoryManager inventoryManager;

    #region Menu Item References
    [SerializeField]
    internal StartMenuActions startMenuActions;
    [SerializeField]
    internal ItemMenuActions itemMenuActions;
    [SerializeField]
    internal AbilitiesMenuActions abilitiesMenuActions;
    [SerializeField]
    internal EquipmentMenuActions equipmentMenuActions;
    [SerializeField]
    internal StatusMenuActions statusMenuActions;
    [SerializeField]
    internal TacticsMenuActions tacticsMenuActions;
    [SerializeField]
    internal JournalMenuActions journalMenuActions;
    [SerializeField]
    internal SaveMenuActions saveMenuActions;
    [SerializeField]
    internal SettingsMenuActions settingsMenuActions;
    #endregion

    [SerializeField]
    internal int currentMenuOpen = 10;
    internal float speed = 0.5f;
    internal bool coroutineRunning = false;
    internal bool closeAllOverride = false;

    internal bool menuFlowIsRunning = false;

    private int heroLineupChangeOne;
    private int heroLineupChangeTwo;


    // UPDATES
    private void Start()
    {
        inputManager = InputManager._instance;
        gameManager = GameManager._instance;
        inventoryManager = InventoryManager._instance;
    }

    // METHODS
    public void MenuSwitchboard(int newMenuToOpen)
    {
        if (currentMenuOpen == newMenuToOpen)
        {
            return;
        }
        CloseSwitchBoard(currentMenuOpen);
        switch(newMenuToOpen)
        {
            case 0:         // Start
                startMenuActions.StartMenuOpen();
                startMenuActions.DisplayPartyHeroes();
                startMenuActions.DisplayMisc();
                currentMenuOpen = 0;
                break;
            case 1:         // Item
                StartCoroutine(itemMenuActions.ItemMenuOpen());
                currentMenuOpen = 1;
                break;
            case 2:         // Spells
                StartCoroutine(abilitiesMenuActions.AbilitiesMenuOpen());
                currentMenuOpen = 2;
                break;
            case 3:         // Equipment
                StartCoroutine(equipmentMenuActions.EquipmentMenuOpen());
                currentMenuOpen = 3;
                break;
            case 4:         // Status
                StartCoroutine(statusMenuActions.StatusMenuOpen());
                currentMenuOpen = 4;
                break;
            case 5:         // Tactics
                StartCoroutine(tacticsMenuActions.TacticsMenuOpen());
                currentMenuOpen = 5;
                break;
            case 6:         // Journal
                StartCoroutine(journalMenuActions.JournalMenuOpen());
                currentMenuOpen = 6;
                break;
            case 7:         // Save
                StartCoroutine(saveMenuActions.SaveMenuOpen());
                currentMenuOpen = 7;
                break;
            case 8:         // Settings
                StartCoroutine(settingsMenuActions.SettingsMenuOpen());
                currentMenuOpen = 8;
                break;
            case 9:         // Quit
                currentMenuOpen = 9;
                QuitMenuOpen();
                break;
        }
        currentMenuOpen = newMenuToOpen;
    }
    public void CloseSwitchBoard(int menuToClose)
    {
        switch (menuToClose)
        {
            case 0:
                StartCoroutine(startMenuActions.StartMenuClose());
                heroLineupChangeOne = 0;
                heroLineupChangeTwo = 0;
                break;
            case 1:
                StartCoroutine(itemMenuActions.ItemMenuClose(closeAllOverride));
                break;
            case 2:
                StartCoroutine(abilitiesMenuActions.AbilitiesMenuClose(closeAllOverride));
                break;
            case 3:
                StartCoroutine(equipmentMenuActions.EquipmentMenuClose(closeAllOverride));
                break;
            case 4:
                StartCoroutine(statusMenuActions.StatusMenuClose(closeAllOverride));
                break;
            case 5:
                StartCoroutine(tacticsMenuActions.TacticsMenuClose(closeAllOverride));
                break;
            case 6:
                StartCoroutine(journalMenuActions.JournalMenuClose(closeAllOverride));
                break;
            case 7:
                StartCoroutine(saveMenuActions.SaveMenuClose(closeAllOverride));
                break;
            case 8:
                StartCoroutine(settingsMenuActions.SettingsMenuClose(closeAllOverride));
                break;
            case 9:
                QuitMenuClose(closeAllOverride);
                break;
        }
    }

    public IEnumerator OpenFirstMenu()
    {
        if (!coroutineRunning)
        {
            coroutineRunning = true;
            MenuSwitchboard(0);
            inputManager.MenuBackground.GetComponent<Image>().DOFade(1, inputManager.menuInputs.speed);
            yield return new WaitForSeconds(speed);
            EventManager._instance.SwitchGameState(GameState.MENU);
            coroutineRunning = false;
            menuFlowIsRunning = false;
        }
    }
    public IEnumerator CloseAllMenus()
    {
        closeAllOverride = true;
        if (!coroutineRunning)
        {
            if (currentMenuOpen == 0)
            {
                inputManager.MenuBackground.GetComponent<Image>().DOFade(0, inputManager.menuInputs.speed);
                CloseSwitchBoard(currentMenuOpen);
                yield return new WaitForSeconds(speed);
                currentMenuOpen = 10;
                EventManager._instance.SwitchGameState(GameState.OVERWORLD);
            }
            else
            {
                closeAllOverride = false;
                CloseSwitchBoard(currentMenuOpen);
            }
        }
        closeAllOverride = false;
        menuFlowIsRunning = false;
    }

    public void ChangePartyLineup(int selectedToChange)
    {
        if(heroLineupChangeOne == 0)
        {
            heroLineupChangeOne = selectedToChange;
            Debug.Log("Set 1!");
        }
        else if(heroLineupChangeTwo == 0)
        {
            heroLineupChangeTwo = selectedToChange;
            Debug.Log("Set 2!");
            PerformSwap(heroLineupChangeOne, heroLineupChangeTwo);
        }
    }
    private void PerformSwap(int heroOne, int heroTwo)
    {
        HeroExtension a = gameManager._PartyLineup[heroOne - 1];
        HeroExtension b = gameManager._PartyLineup[heroTwo - 1];

        gameManager._PartyLineup[heroOne - 1] = b;
        gameManager._PartyLineup[heroTwo - 1] = a;
        startMenuActions.DisplayPartyHeroes();

        heroLineupChangeOne = 0;
        heroLineupChangeTwo = 0;
    }
    #region Abilities Menu
    #endregion
    #region Journal Menu
    #endregion
    #region Settings Menu
    #endregion
    #region Quit Menu
    private void QuitMenuOpen()
    {
        inputManager.MenuItems[9].SetActive(true);
    }
    private void QuitMenuClose(bool closeAllOverride)
    {
        inputManager.MenuItems[9].SetActive(false);
        if (!closeAllOverride)
        {
            startMenuActions.StartMenuOpen();
            currentMenuOpen = 0;
        }
    }
    private void QuitApplication()
    {
        Application.Quit();
    }
    #endregion
}
