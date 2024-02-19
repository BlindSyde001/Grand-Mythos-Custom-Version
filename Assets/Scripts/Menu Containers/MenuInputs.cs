using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MenuInputs : MonoBehaviour
{
    // VARIABLES
    public InputManager InputManager { get; private set; }
    public GameManager GameManager { get; private set; }

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
    internal MenuContainer currentMenuOpen;
    internal float speed = 0.5f;
    internal bool coroutineRunning = false;

    internal bool menuFlowIsRunning = false;

    private int heroLineupChangeOne;
    private int heroLineupChangeTwo;


    // UPDATES
    private void Start()
    {
        InputManager = InputManager._instance;
        GameManager = GameManager._instance;
    }

    // METHODS
    public void MenuSwitchboard(MenuContainer newMenuToOpen)
    {
        if (currentMenuOpen == newMenuToOpen)
        {
            return;
        }
        if (currentMenuOpen != null)
            CloseSwitchBoard(currentMenuOpen);
        StartCoroutine(newMenuToOpen.Open(this).GetEnumerator());
        currentMenuOpen = newMenuToOpen;
    }

    public void CloseSwitchBoard(MenuContainer menuToClose) => CloseSwitchBoard(menuToClose, false);
    public void CloseSwitchBoard(MenuContainer menuToClose, bool closeAllOverride)
    {
        if (menuToClose is StartMenuActions)
        {
            heroLineupChangeOne = 0;
            heroLineupChangeTwo = 0;
        }

        StartCoroutine(CloseWithOverride(menuToClose, closeAllOverride));

        IEnumerator CloseWithOverride(MenuContainer container, bool closeAllOverride)
        {
            foreach (var va in container.Close(this))
            {
                yield return va;
            }
            if (!closeAllOverride && startMenuActions != container)
            {
                foreach (var val in startMenuActions.Open(this))
                {
                    yield return val;
                }
                yield return new WaitForSeconds(speed);
                currentMenuOpen = startMenuActions;
            }
        }
    }

    public IEnumerator OpenFirstMenu()
    {
        if (coroutineRunning)
            yield break;

        coroutineRunning = true;
        MenuSwitchboard(startMenuActions);
        InputManager.MenuBackground.GetComponent<Image>().DOFade(1, InputManager.menuInputs.speed);
        yield return new WaitForSeconds(speed);
        EventManager._instance.SwitchGameState(GameState.MENU);
        coroutineRunning = false;
        menuFlowIsRunning = false;
    }
    public IEnumerator CloseAllMenus()
    {
        if (!coroutineRunning)
        {
            if (currentMenuOpen == startMenuActions)
            {
                InputManager.MenuBackground.GetComponent<Image>().DOFade(0, InputManager.menuInputs.speed);
                CloseSwitchBoard(currentMenuOpen, true);
                yield return new WaitForSeconds(speed);
                currentMenuOpen = null;
                EventManager._instance.SwitchGameState(GameState.OVERWORLD);
            }
            else
            {
                CloseSwitchBoard(currentMenuOpen, false);
            }
        }
        menuFlowIsRunning = false;
    }

    public void ChangePartyLineup(int selectedToChange)
    {
        if(heroLineupChangeOne == 0)
        {
            heroLineupChangeOne = selectedToChange;
        }
        else if(heroLineupChangeTwo == 0)
        {
            heroLineupChangeTwo = selectedToChange;
            PerformSwap(heroLineupChangeOne, heroLineupChangeTwo);
        }
    }
    private void PerformSwap(int heroOne, int heroTwo)
    {
        HeroExtension a = GameManager._PartyLineup[heroOne - 1];
        HeroExtension b = GameManager._PartyLineup[heroTwo - 1];

        GameManager._PartyLineup[heroOne - 1] = b;
        GameManager._PartyLineup[heroTwo - 1] = a;
        startMenuActions.DisplayPartyHeroes();

        heroLineupChangeOne = 0;
        heroLineupChangeTwo = 0;
    }
    private void QuitApplication()
    {
        Application.Quit();
    }
}
