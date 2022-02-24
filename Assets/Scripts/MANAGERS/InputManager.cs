using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    // VARIABLES
    public static InputManager _instance;
    [SerializeField]
    internal PlayerInput playerInput;
    [SerializeField]
    internal MenuInputs menuInputs;
    #region Menu Items
    [SerializeField]
    internal GameObject MenuBackground;
    [SerializeField]
    internal List<GameObject> MenuItems;
    #endregion

    // UPDATES
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }

    // METHODS
    #region INPUT COMMANDS
    public void StartMenuOpen(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!menuInputs.menuFlowIsRunning)
            {
                menuInputs.menuFlowIsRunning = true;
                StartCoroutine(menuInputs.OpenFirstMenu());
            }
        }
    }
    public void StartMenuClose(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!menuInputs.menuFlowIsRunning)
            {
                menuInputs.menuFlowIsRunning = true;
                StartCoroutine(menuInputs.CloseAllMenus());
            }
        }
    }
    #endregion
}