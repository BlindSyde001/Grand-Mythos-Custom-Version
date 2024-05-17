using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;

public class MenuInputs : MonoBehaviour
{
    public GameManager GameManager { get; private set; }

    [Required]
    public InputActionReference Open, Close;

    [SerializeField]
    internal StartMenuActions startMenuActions;
    [SerializeField]
    public Image MenuBackground;

    public MenuContainer CurrentMenuOpen;
    public float Speed = 0.5f;

    bool _busySwitching;


    // UPDATES
    void Start()
    {
        GameManager = GameManager.Instance;
        CurrentMenuOpen = null;
        startMenuActions.gameObject.SetActive(false);
    }

    void Update()
    {
        if (_busySwitching == false) // Prevents creating a long queue of open-close when spamming the button
        {
            if (Open.action.WasPressedThisFrame())
                StartCoroutine(QueueSwitchTo(startMenuActions));

            if (Close.action.WasPressedThisFrame())
            {
                if (CurrentMenuOpen != startMenuActions) // Switch towards start menu if in another submenu
                    StartCoroutine(QueueSwitchTo(startMenuActions));
                else // Close if we're on the start menu
                    StartCoroutine(QueueSwitchTo(null));
            }
        }
    }

    // METHODS
    public void MenuSwitchboard(MenuContainer newMenuToOpen) => StartCoroutine(QueueSwitchTo(newMenuToOpen));

    public void CloseSwitchBoard(MenuContainer menuToClose) => StartCoroutine(QueueSwitchTo(startMenuActions));

    public void TryCloseMenu()
    {
        if (_busySwitching)
            return;
        StartCoroutine(QueueSwitchTo(null));
    }

    public void PreviousMenu()
    {
        if (CurrentMenuOpen is StartMenuActions)
            StartCoroutine(QueueSwitchTo(null));
        else if (CurrentMenuOpen)
            StartCoroutine(QueueSwitchTo(startMenuActions));
    }

    /// <summary>
    /// Will switch to this menu, if another menu is already being switched to, this will wait for that switch to complete before taking over
    /// </summary>
    IEnumerator QueueSwitchTo(MenuContainer menu)
    {
        while (_busySwitching)
            yield return null;

        foreach (object yieldType in TrySwitchTo(menu))
            yield return yieldType;
    }

    /// <summary>
    /// Tries to switch to this menu, if another switch is running, this will exit out early without switching
    /// </summary>
    IEnumerable TrySwitchTo(MenuContainer to)
    {
        if (_busySwitching)
            yield break;

        if (CurrentMenuOpen == to)
            yield break;

        try
        {
            _busySwitching = true;
            var from = CurrentMenuOpen;
            CurrentMenuOpen = to;

            if (from == null)
            {
                InputManager.PushGameState(GameState.Menu, this);
                MenuBackground.DOFade(1, Speed);
            }
            if (to == null)
            {
                InputManager.PopGameState(this);
                MenuBackground.DOFade(0, Speed);
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
            }
        }
        finally
        {
            _busySwitching = false;
        }
    }

    void QuitApplication()
    {
        Application.Quit();
    }
}
