using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // VARIABLES
    public static InputManager _instance;
    internal PlayerInput playerInput;

    #region Menu Items
    [SerializeField]
    private GameObject MenuGraphic;
    [SerializeField]
    private List<GameObject> MenuItems;
    #endregion

    // UPDATES
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
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
    public void CmdOpenMenu(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            EventManager._instance.SwitchGameState(GameState.MENU);
            MenuGraphic.SetActive(true);
            foreach (GameObject item in MenuItems)
            {
                item.SetActive(false);
            }
            MenuItems[0].SetActive(true);
        }
    }
    public void CmdCloseMenu(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            EventManager._instance.SwitchGameState(GameState.OVERWORLD);
            foreach (GameObject item in MenuItems)
            {
                item.SetActive(false);
            }
            MenuGraphic.SetActive(false);
        }
    }
}
