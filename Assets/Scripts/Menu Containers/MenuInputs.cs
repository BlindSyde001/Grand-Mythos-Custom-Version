using System.Collections;
using System.Collections.Generic;
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
    ((List<HeroExtension> collection, int index) sourceA, (List<HeroExtension> collection, int index) sourceB) lineupChange;


    // UPDATES
    void Start()
    {
        InputManager = InputManager.Instance;
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
            lineupChange = default;
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
        InputManager.MenuBackground.GetComponent<Image>().DOFade(1, InputManager.MenuInputs.speed);
        yield return new WaitForSeconds(speed);
        InputManager.Instance.PushGameState(GameState.Menu, this);
        coroutineRunning = false;
        menuFlowIsRunning = false;
    }
    public IEnumerator CloseAllMenus()
    {
        if (!coroutineRunning)
        {
            if (currentMenuOpen == startMenuActions)
            {
                InputManager.MenuBackground.GetComponent<Image>().DOFade(0, InputManager.MenuInputs.speed);
                CloseSwitchBoard(currentMenuOpen, true);
                yield return new WaitForSeconds(speed);
                currentMenuOpen = null;
                InputManager.Instance.PopGameState(this);
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
        ChangePartyLineup((GameManager.PartyLineup, selectedToChange-1));
    }

    public void ChangePartyLineupFromReserve(int selectedToChange)
    {
        ChangePartyLineup((GameManager.ReservesLineup, selectedToChange-1));
    }

    void ChangePartyLineup((List<HeroExtension> _PartyLineup, int selectedToChange) data)
    {
        if (lineupChange.sourceA.collection == null)
        {
            lineupChange.sourceA = data;
        }
        else
        {
            lineupChange.sourceB = data;


            var (sourceA, sourceB) = lineupChange;
            lineupChange = default;

            var elementA = sourceA.collection[sourceA.index];
            var elementB = sourceB.collection[sourceB.index];

            sourceA.collection[sourceA.index] = elementB;
            sourceB.collection[sourceB.index] = elementA;

            startMenuActions.DisplayPartyHeroes();
        }
    }

    void QuitApplication()
    {
        Application.Quit();
    }
}
