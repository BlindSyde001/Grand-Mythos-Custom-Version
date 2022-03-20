using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;


public class SaveMenuActions : MonoBehaviour
{
    //  VARIABLES
    private MenuInputs menuInputs;
    private InputManager inputManager;
    private GameManager gameManager;
    private InventoryManager inventoryManager;

    public GameObject LoadList;
    public List<SaveFileButton> SavedFiles;
    public TextMeshProUGUI savedtext;

    // UPDATES
    private void Start()
    {
        menuInputs = FindObjectOfType<MenuInputs>();
        inputManager = InputManager._instance;
        gameManager = GameManager._instance;
        inventoryManager = InventoryManager._instance;
    }

    // METHODS
    public void OpenLoadFiles()
    {
        string[] readFiles = GetFileNames(Application.persistentDataPath + "/Save Files", "*.json");
        // Interpret the Data on the read files
        for (int i = 0; i < readFiles.Length; i++)
        {
            // Open data packet from files
            SaveData SD = SaveManager.LoadFromFile(i);

            // Set the Lineup of heroes
            if (SD != null)
            {
                List<HeroExtension> tempHero = new List<HeroExtension>();
                for (int j = 0; j < SD.lineupSave.Count; j++)
                {
                    tempHero.Add(gameManager._AllPartyMembers[SD.lineupSave[j]]);
                }

                // foreach party member, display their icon in order
                for (int k = 0; k < SD.lineupSave.Count; k++)
                {
                    SavedFiles[i].characterPortraits[k].sprite = tempHero[k].charPortrait;
                }
            }
            // Display name of the file
            SavedFiles[i].fileName.text = "Saved Game " + i;

            int hr = SD.playTimeData / 3600 % 24;
            int min = SD.playTimeData / 60 % 60;
            int sec = SD.playTimeData % 60;
            SavedFiles[i].timePlayed.text = ((hr < 10) ? ("0" + hr) : hr) + ":" +
                                            ((min < 10) ? ("0" + min) : min) + ":" +
                                            ((sec < 10) ? ("0" + sec) : sec);
            SavedFiles[i].moneyAcquired.text = SD.inventorySaveData.CreditsAmountData.ToString() + " Credits";
        }

        for (int j = 0; j < SavedFiles.Count; j++)
        {
            SavedFiles[j].GetComponent<Button>().onClick.RemoveAllListeners();
            
            int q = j;
            SavedFiles[j].GetComponent<Button>().onClick.AddListener(delegate { ClickSave(q); });
        }
    }
    private string[] GetFileNames(string path, string filter)
    {
        string[] files = Directory.GetFiles(path, filter);
        for (int i = 0; i < files.Length; i++)
        {
            files[i] = Path.GetFileName(files[i]);
        }
        return files;
    }

    public void ClickSave(int SaveFileNumber)
    {
        #region Positional & Zone Data
        gameManager.LastKnownScene = SceneManager.GetActiveScene().name;
        gameManager.LastKnownPosition = FindObjectOfType<OverworldPlayerCircuit>().transform.position;
        gameManager.LastKnownRotation = FindObjectOfType<OverworldPlayerCircuit>().transform.rotation;
        gameManager.LastKnownReferenceDirection = Camera.main.GetComponent<CameraManager>().ReferenceDirections.IndexOf(FindObjectOfType<OverworldPlayerCircuit>().referenceDirection);

        SaveData.current.savedScene = gameManager.LastKnownScene;
        SaveData.current.overworldPos = gameManager.LastKnownPosition;
        SaveData.current.overworldRot = gameManager.LastKnownRotation;
        SaveData.current.overworldRefDirection = gameManager.LastKnownReferenceDirection;
        #endregion
        #region Lineup Data
        // Set x as Index Number of Hero in AllList, so that you can pull that hero by Index when making the Lineup
        SaveData.current.lineupSave.Clear();
        foreach (HeroExtension hero in GameManager._instance._PartyLineup)
        {
            int x = GameManager._instance._AllPartyMembers.IndexOf(hero);
            SaveData.current.lineupSave.Add(x);
        }
        #endregion
        #region Hero Data
        // Creating Data packets per hero and saving those
        SaveData.current.heroSaveData.Clear();
        foreach (HeroExtension hero in GameManager._instance._AllPartyMembers)
        {
            SerializableHero heroSave = new();
            heroSave.totalExperienceSave = hero._TotalExperience;

            heroSave.weaponIDSave = hero._Weapon._ItemID;
            heroSave.armourIDSave = hero._Armour._ItemID;
            if (hero._AccessoryOne != null)
            {
                heroSave.accessoryOneIDSave = hero._AccessoryOne._ItemID;
                heroSave.accessoryOneSave = true;
            }
            if (hero._AccessoryTwo != null)
            {
                heroSave.accessoryTwoIDSave = hero._AccessoryTwo._ItemID;
                heroSave.accessoryTwoSave = true;
            }
            heroSave.weaponSave = hero._Weapon.weaponType.ToString();
            heroSave.armourSave = hero._Armour.armourType.ToString();

            heroSave.currentHPData = hero._CurrentHP;
            heroSave.currentMPData = hero._CurrentMP;

            SerializableTacticController tacticSave = new();
            // Go through all 10 Tactics, add info to the tacticSave
            for (int i = 0; i < hero.myTacticController._TacticsList.Count; i++)
            {
                tacticSave.tacticToggleList[i] = hero.myTacticController._TacticsList[i].isTurnedOn;
                if(hero.myTacticController._TacticsList[i]._Condition != null)
                {
                    tacticSave.tacticCndList[i] = hero.myTacticController._TacticsList[i]._Condition.name;
                }
                if(hero.myTacticController._TacticsList[i]._Actions != null)
                {
                    SerializableTacticActions tacticActions = new();
                    for(int j = 0; j < hero.myTacticController._TacticsList[j]._Actions.Count; j++)
                    {
                        if (hero.myTacticController._TacticsList[i]._Actions[j] != null)
                        {
                            tacticActions.tacticActions[j] = hero.myTacticController._TacticsList[i]._Actions[j].Name;
                            tacticActions.tacticActionTypes[j] = hero.myTacticController._TacticsList[i]._Actions[j].ActionType.ToString();
                            //tacticSave.tacticActionCapsulesList[i].tacticActionTypes[j] = hero.myTacticController._TacticsList[i]._Actions[j].ActionType.ToString();
                            //tacticSave.tacticActionCapsulesList[i].tacticActions[j] = hero.myTacticController._TacticsList[i]._Actions[j].Name;
                            //tacticSave.tacticActionCapsulesList[i] = tacticActions;
                        }
                    }
                    tacticSave.tacticActionCapsulesList.Add(tacticActions);
                }
            }

            SaveData.current.heroSaveData.Add(heroSave);
            SaveData.current.heroTacticData.Add(tacticSave);
        }
        #endregion
        #region Inventory Data
        SaveData.current.inventorySaveData = new();

        foreach (ItemCapsule a in inventoryManager.ConsumablesInBag)
        {
            SaveData.current.inventorySaveData.ConsumablesIdData.Add(a.thisItem._ItemID);
            SaveData.current.inventorySaveData.ConsumablesAmountData.Add(a.ItemAmount);
        }
        foreach (ItemCapsule a in inventoryManager.EquipmentInBag)
        {
            SaveData.current.inventorySaveData.EquipmentNameData.Add(a.thisItem.name);
            SaveData.current.inventorySaveData.EquipmentIdData.Add(a.ItemID);
            SaveData.current.inventorySaveData.EquipmentAmountData.Add(a.ItemAmount);
        }
        foreach (ItemCapsule a in inventoryManager.KeyItemsInBag)
        {
            SaveData.current.inventorySaveData.KeyItemsIdData.Add(a.ItemID);
        }
        foreach (ItemCapsule a in inventoryManager.LootInBag)
        {
            SaveData.current.inventorySaveData.LootIdData.Add(a.ItemID);
            SaveData.current.inventorySaveData.LootAmountData.Add(a.ItemAmount);
        }
        foreach (Condition a in inventoryManager.ConditionsAcquired)
        {
            SaveData.current.inventorySaveData.ConditionsIdData.Add(a.cndID);
        }
        SaveData.current.inventorySaveData.CreditsAmountData = inventoryManager.creditsInBag;
        #endregion
        #region Time Data
        SaveData.current.playTimeData = FindObjectOfType<InGameClock>().playTime;
        #endregion
        SaveManager.SaveToFile(SaveData.current, SaveFileNumber);
        StartCoroutine(SavedGameGraphic());
    }
    private IEnumerator SavedGameGraphic()
    {
        savedtext.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        savedtext.gameObject.SetActive(false);
    }

    internal IEnumerator SaveMenuOpen()
    {
        if (!menuInputs.coroutineRunning)
        {
            yield return new WaitForSeconds(menuInputs.speed);
            inputManager.MenuItems[7].SetActive(true);
            inputManager.MenuItems[7].transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.speed);
            inputManager.MenuItems[7].transform.GetChild(1).DOLocalMove(new Vector3(190, 0, 0), menuInputs.speed);
            OpenLoadFiles();
        }
    }
    internal IEnumerator SaveMenuClose(bool closeAllOverride)
    {
        if (!menuInputs.coroutineRunning)
        {
            menuInputs.coroutineRunning = true;
            inputManager.MenuItems[7].transform.GetChild(0).DOLocalMove(new Vector3(-1200, 480, 0), menuInputs.speed);
            inputManager.MenuItems[7].transform.GetChild(1).DOLocalMove(new Vector3(1750, 0, 0), menuInputs.speed);
            yield return new WaitForSeconds(menuInputs.speed);
            inputManager.MenuItems[7].SetActive(false);
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