using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SettingsMenuActions : MonoBehaviour
{

    // VARIABLES
    private InputManager inputManager;
    private MenuInputs menuInputs;
    // UPDATES
    private void Start()
    {
        inputManager = InputManager._instance;
        menuInputs = FindObjectOfType<MenuInputs>();
    }

    // METHODS
    internal IEnumerator SettingsMenuOpen()
    {
        if (!menuInputs.coroutineRunning)
        {
            yield return new WaitForSeconds(menuInputs.speed);
            inputManager.MenuItems[8].SetActive(true);
            inputManager.MenuItems[8].transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.speed);
        }
    }
    internal IEnumerator SettingsMenuClose(bool closeAllOverride)
    {
        if (!menuInputs.coroutineRunning)
        {
            menuInputs.coroutineRunning = true;
            inputManager.MenuItems[8].transform.GetChild(0).DOLocalMove(new Vector3(-1200, 480, 0), menuInputs.speed);
            yield return new WaitForSeconds(menuInputs.speed);
            inputManager.MenuItems[8].SetActive(false);
            menuInputs.coroutineRunning = false;
        }
        if (!closeAllOverride)
        {
            menuInputs.startMenuActions.StartMenuOpen();
            yield return new WaitForSeconds(menuInputs.speed);
            menuInputs.currentMenuOpen = 0;
        }
    }
}
