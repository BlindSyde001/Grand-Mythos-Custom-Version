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

    [SerializeField]
    internal StartMenuActions startMenuActions;

    public MenuContainer CurrentMenuOpen;
    public float Speed = 0.5f;

    ((List<HeroExtension> collection, int index) sourceA, (List<HeroExtension> collection, int index) sourceB) _lineupChange;
    bool _busySwitching;


    // UPDATES
    void Start()
    {
        InputManager = InputManager.Instance;
        GameManager = GameManager.Instance;
        CurrentMenuOpen = null;
    }

    // METHODS
    public void MenuSwitchboard(MenuContainer newMenuToOpen) => StartCoroutine(SwitchTo(newMenuToOpen));

    public void CloseSwitchBoard(MenuContainer menuToClose) => StartCoroutine(SwitchTo(startMenuActions));

    public void PreviousMenu()
    {
        if (CurrentMenuOpen is StartMenuActions)
            StartCoroutine(CloseAllMenus());
        else if (CurrentMenuOpen)
            StartCoroutine(SwitchTo(startMenuActions));
    }

    public IEnumerator OpenFirstMenu()
    {
        InputManager.Instance.PushGameState(GameState.Menu, this);
        InputManager.MenuBackground.GetComponent<Image>().DOFade(1, Speed);
        for (var e = SwitchTo(startMenuActions); e.MoveNext(); )
            yield return e.Current;
    }

    public IEnumerator CloseAllMenus()
    {
        for (var e = SwitchTo(null); e.MoveNext(); )
            yield return e.Current;
    }

    IEnumerator SwitchTo(MenuContainer to)
    {
        if (_busySwitching)
            yield break;

        if (CurrentMenuOpen == to)
            yield break;

        _busySwitching = true;
        var from = CurrentMenuOpen;
        CurrentMenuOpen = to;

        if (from is StartMenuActions)
            _lineupChange = default;

        if (from == null)
        {
            InputManager.Instance.PushGameState(GameState.Menu, this);
            InputManager.MenuBackground.GetComponent<Image>().DOFade(1, Speed);
        }
        if (to == null)
        {
            InputManager.Instance.PopGameState(this);
            InputManager.MenuBackground.GetComponent<Image>().DOFade(0, Speed);
        }

#warning would be nice to manually parse this enum to accelerate it whenever we have a command for a new switch comming in that way we don't block any new commands
        if (from != null)
        {
            foreach (var yield in from.Close(this))
                yield return yield;
        }

        if (to != null)
        {
            foreach (var yield in to.Open(this))
                yield return yield;

            if (to.GetComponentInChildren<Button>() is { } button)
                button.Select();
        }


        _busySwitching = false;
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
        if (_lineupChange.sourceA.collection == null)
        {
            _lineupChange.sourceA = data;
        }
        else
        {
            _lineupChange.sourceB = data;


            var (sourceA, sourceB) = _lineupChange;
            _lineupChange = default;

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
