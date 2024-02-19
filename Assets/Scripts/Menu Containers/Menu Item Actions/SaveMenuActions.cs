using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;


public class SaveMenuActions : MenuContainer
{
    public GameObject LoadList;
    public List<SaveFileButton> SavedFiles;
    public TextMeshProUGUI savedtext;

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
                    tempHero.Add(GameManager._AllPartyMembers[SD.lineupSave[j]]);
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
        GameManager.LastKnownScene = SceneManager.GetActiveScene().name;
        GameManager.LastKnownPosition = FindObjectOfType<OverworldPlayerControlsNode>().transform.position;
        GameManager.LastKnownRotation = FindObjectOfType<OverworldPlayerControlsNode>().transform.rotation;

        SaveData.current.savedScene = GameManager.LastKnownScene;
        SaveData.current.overworldPos = GameManager.LastKnownPosition;
        SaveData.current.overworldRot = GameManager.LastKnownRotation;
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

            heroSave.weaponIDSave = hero._Weapon.guid;
            heroSave.armourIDSave = hero._Armour.guid;
            if (hero._AccessoryOne != null)
            {
                heroSave.accessoryOneIDSave = hero._AccessoryOne.guid;
                heroSave.accessoryOneSave = true;
            }
            if (hero._AccessoryTwo != null)
            {
                heroSave.accessoryTwoIDSave = hero._AccessoryTwo.guid;
                heroSave.accessoryTwoSave = true;
            }
            heroSave.weaponSave = hero._Weapon.weaponType.ToString();
            heroSave.armourSave = hero._Armour.armourType.ToString();

            heroSave.currentHPData = hero._CurrentHP;
            heroSave.currentMPData = hero._CurrentMP;

            SerializableTacticController tacticSave = new();

            SaveData.current.heroSaveData.Add(heroSave);
            SaveData.current.heroTacticData.Add(tacticSave);
        }
        #endregion
        #region Inventory Data
        SaveData.current.inventorySaveData = new();

        foreach (ItemCapsule a in InventoryManager.ConsumablesInBag)
        {
            SaveData.current.inventorySaveData.ConsumablesIdData.Add(a.thisItem.guid);
            SaveData.current.inventorySaveData.ConsumablesAmountData.Add(a.ItemAmount);
        }
        foreach (ItemCapsule a in InventoryManager.EquipmentInBag)
        {
            SaveData.current.inventorySaveData.EquipmentNameData.Add(((Object)a.thisItem).name);
            SaveData.current.inventorySaveData.EquipmentIdData.Add(a.thisItem.guid);
            SaveData.current.inventorySaveData.EquipmentAmountData.Add(a.ItemAmount);
        }
        foreach (ItemCapsule a in InventoryManager.KeyItemsInBag)
        {
            SaveData.current.inventorySaveData.KeyItemsIdData.Add(a.thisItem.guid);
        }
        foreach (ItemCapsule a in InventoryManager.LootInBag)
        {
            SaveData.current.inventorySaveData.LootIdData.Add(a.thisItem.guid);
            SaveData.current.inventorySaveData.LootAmountData.Add(a.ItemAmount);
        }
        SaveData.current.inventorySaveData.CreditsAmountData = InventoryManager.creditsInBag;
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

    public override IEnumerable Open(MenuInputs menuInputs)
    {
        if (!menuInputs.coroutineRunning)
        {
            yield return new WaitForSeconds(menuInputs.speed);
            gameObject.SetActive(true);
            gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.speed);
            gameObject.transform.GetChild(1).DOLocalMove(new Vector3(190, 0, 0), menuInputs.speed);
            OpenLoadFiles();
        }
    }
    public override IEnumerable Close(MenuInputs menuInputs)
    {
        if (!menuInputs.coroutineRunning)
        {
            menuInputs.coroutineRunning = true;
            gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-1200, 480, 0), menuInputs.speed);
            gameObject.transform.GetChild(1).DOLocalMove(new Vector3(1750, 0, 0), menuInputs.speed);
            yield return new WaitForSeconds(menuInputs.speed);
            gameObject.SetActive(false);
            menuInputs.coroutineRunning = false;
        }
    }
}