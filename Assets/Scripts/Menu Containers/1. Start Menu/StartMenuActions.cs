using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StartMenuActions : MonoBehaviour
{
    // VARIABLES
    private MenuInputs menuInputs;
    private InputManager inputManager;
    private GameManager gameManager;
    private InventoryManager inventoryManager;

    [SerializeField]
    private List<PartyContainer> displayList;
    [SerializeField]
    private MiscContainer miscList;
    [SerializeField]
    private InGameClock inGameClock;

    // UPDATES
    private void Start()
    {
        menuInputs = FindObjectOfType<MenuInputs>();
        inputManager = InputManager._instance;
        gameManager = GameManager._instance;
        inventoryManager = InventoryManager._instance;
    }

    private void LateUpdate()
    {
        miscList.miscTime.text = ((inGameClock.hour < 10)? ("0" + inGameClock.hour) : inGameClock.hour) + ":" + 
                                 ((inGameClock.minute < 10)? ("0" + inGameClock.minute) : inGameClock.minute) + ":" + 
                                 ((inGameClock.second < 10)? ("0" + inGameClock.second) : inGameClock.second); 
    }
    // METHODS
    internal void StartMenuOpen()
    {
        foreach (GameObject a in inputManager.MenuItems)
        {
            a.SetActive(false);
        }
        inputManager.MenuItems[0].SetActive(true);
        inputManager.MenuItems[0].transform.GetChild(0).DOLocalMove(new Vector3(-740, 150, 0), menuInputs.speed);
        inputManager.MenuItems[0].transform.GetChild(1).DOLocalMove(new Vector3(0, 0, 0), menuInputs.speed);
        inputManager.MenuItems[0].transform.GetChild(2).DOLocalMove(new Vector3(200, 30, 0), menuInputs.speed);
    }
    internal IEnumerator StartMenuClose()
    {
        if (!menuInputs.coroutineRunning)
        {
            inputManager.MenuItems[0].transform.GetChild(0).DOLocalMove(new Vector3(-1200, 150, 0), menuInputs.speed);
            inputManager.MenuItems[0].transform.GetChild(1).DOLocalMove(new Vector3(0, -150, 0), menuInputs.speed);
            inputManager.MenuItems[0].transform.GetChild(2).DOLocalMove(new Vector3(1700, 30, 0), menuInputs.speed);
            yield return new WaitForSeconds(menuInputs.speed);
            inputManager.MenuItems[0].SetActive(false);
        }
    }
    internal void DisplayPartyHeroes()
    {
        foreach (PartyContainer a in displayList)
        {
            a.gameObject.SetActive(false);
        }
        for (int i = 0; i < gameManager._PartyLineup.Count; i++)
        {
            displayList[i].gameObject.SetActive(true);
            displayList[i].displayName.text = gameManager._PartyLineup[i].charName;

            displayList[i].displayBanner.sprite = gameManager._PartyLineup[i].charBanner;

            displayList[i].displayLevel.text = gameManager._PartyLineup[i]._Level.ToString();

            displayList[i].displayEXPBar.fillAmount =
                (float)gameManager._PartyLineup[i]._TotalExperience /
                gameManager._PartyLineup[i].ExperienceThreshold;

            displayList[i].displayHP.text =
                gameManager._PartyLineup[i]._CurrentHP.ToString() + " / " +
                gameManager._PartyLineup[i].MaxHP.ToString();
        }
    }
    internal void DisplayMisc()
    {
        SceneInformation a = FindObjectOfType<SceneInformation>();
        miscList.miscArea.text = a.areaName;
        miscList.miscZone.text = a.zoneName;
        miscList.miscCurrency.text = inventoryManager.creditsInBag.ToString();
    }
}
