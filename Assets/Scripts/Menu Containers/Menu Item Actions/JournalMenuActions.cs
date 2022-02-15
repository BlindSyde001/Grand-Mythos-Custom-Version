using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class JournalMenuActions : MonoBehaviour
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
    internal IEnumerator JournalMenuOpen()
    {
        if (!menuInputs.coroutineRunning)
        {
            yield return new WaitForSeconds(menuInputs.speed);
            inputManager.MenuItems[6].SetActive(true);
            inputManager.MenuItems[6].transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.speed);
        }
    }
    internal IEnumerator JournalMenuClose(bool closeAllOverride)
    {
        if (!menuInputs.coroutineRunning)
        {
            menuInputs.coroutineRunning = true;
            inputManager.MenuItems[6].transform.GetChild(0).DOLocalMove(new Vector3(-1200, 480, 0), menuInputs.speed);
            yield return new WaitForSeconds(menuInputs.speed);
            inputManager.MenuItems[6].SetActive(false);
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
