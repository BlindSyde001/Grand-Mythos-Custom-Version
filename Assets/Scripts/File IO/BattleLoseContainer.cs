using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleLoseContainer : MonoBehaviour
{
    // VARIABLES
    private InventoryManager inventoryManager;
    private GameManager gameManager;

    public GameObject LoadList;
    public List<SaveFileButton> SavedFiles;

    // UPDATES
    private void Start()
    {
        inventoryManager = InventoryManager._instance;
        gameManager = GameManager._instance;
    }

    // METHODS
    public void QuitGame()
    {
        Application.Quit();
    }
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
                List<HeroExtension> tempHero = new();
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

    public void LoadGameData(int FileNumber)
    {
        var SD = SaveManager.LoadFromFile(FileNumber);

        #region Positional Data
        gameManager.LastKnownScene = SD.savedScene;
        gameManager.LastKnownPosition = SD.overworldPos;
        gameManager.LastKnownRotation = SD.overworldRot;
        #endregion
        #region Lineup Data
        gameManager._PartyLineup.Clear();
        for (int i = 0; i < SD.lineupSave.Count; i++)
        {
            gameManager._PartyLineup.Add(gameManager._AllPartyMembers[SD.lineupSave[i]]);
        }
        #endregion
        #region Hero Data
        // HERO LIST
        for (int i = 0; i < gameManager._AllPartyMembers.Count; i++)
        {
            gameManager._AllPartyMembers[i].Experience = SD.heroSaveData[i].totalExperienceSave;
            gameManager._AllPartyMembers[i].InitializeLevel();
            SaveManager.LoadWeaponData(SD, i);
            SaveManager.LoadArmourData(SD, i);
            SaveManager.LoadAccessoryData(SD, i);
        }

        for (int i = 0; i < gameManager._PartyLineup.Count; i++)
        {
            gameManager._PartyLineup[i].CurrentHP = SD.heroSaveData[i].currentHPData;
            gameManager._PartyLineup[i].CurrentMP = SD.heroSaveData[i].currentMPData;
        }
        #endregion
        #region Inventory Data
        inventoryManager.ConsumablesInBag.Clear();
        inventoryManager.EquipmentInBag.Clear();
        inventoryManager._WeaponsInBag.Clear();
        inventoryManager._ArmourInBag.Clear();
        inventoryManager._AccessoryInBag.Clear();
        inventoryManager.LootInBag.Clear();
        inventoryManager.KeyItemsInBag.Clear();
        inventoryManager.ConditionsAcquired.Clear();

        // Add Consumables into Bag
        for (int i = 0; i < SD.inventorySaveData.ConsumablesIdData.Count; i++)
        {
            Consumable newCon = gameManager._ConsumablesDatabase.Find(x => x.guid == SD.inventorySaveData.ConsumablesIdData[i]);
            ItemCapsule itemCapsule = new();
            itemCapsule.thisItem = newCon;
            itemCapsule.ItemAmount = SD.inventorySaveData.ConsumablesAmountData[i];
            inventoryManager.ConsumablesInBag.Add(itemCapsule);
        }
        // Add Equipment into Bag as well as Categorised into Weapons / Armour / Accessories
        for (int i = 0; i < SD.inventorySaveData.EquipmentNameData.Count; i++)
        {
            Equipment newEquip;
            ItemCapsule itemCapsule = new();
            switch (SD.inventorySaveData.EquipmentNameData[i])
            {
                #region WEAPONS DATA
                #region Gun
                case string a when a.Contains("Gun"):
                    newEquip = gameManager._GunsDatabase.Find(x => x.guid == SD.inventorySaveData.EquipmentIdData[i]);

                    itemCapsule.thisItem = newEquip;
                    itemCapsule.ItemAmount = SD.inventorySaveData.EquipmentAmountData[i];
                    inventoryManager.EquipmentInBag.Add(itemCapsule);

                    Weapon gun = newEquip as Weapon;
                    itemCapsule.thisItem = gun;
                    inventoryManager._WeaponsInBag.Add(itemCapsule);
                    break;
                #endregion
                #region Warhammer
                case string a when a.Contains("Warhammer"):
                    newEquip = gameManager._WarhammersDatabase.Find(x => x.guid == SD.inventorySaveData.EquipmentIdData[i]);

                    itemCapsule.thisItem = newEquip;
                    itemCapsule.ItemAmount = SD.inventorySaveData.EquipmentAmountData[i];
                    inventoryManager.EquipmentInBag.Add(itemCapsule);

                    Weapon warHammer = newEquip as Weapon;
                    itemCapsule.thisItem = warHammer;
                    inventoryManager._WeaponsInBag.Add(itemCapsule);
                    break;
                #endregion
                #region Power Glove
                case string a when a.Contains("Power Glove"):
                    newEquip = gameManager._PowerGlovesDatabase.Find(x => x.guid == SD.inventorySaveData.EquipmentIdData[i]);

                    itemCapsule.thisItem = newEquip;
                    itemCapsule.ItemAmount = SD.inventorySaveData.EquipmentAmountData[i];
                    inventoryManager.EquipmentInBag.Add(itemCapsule);

                    Weapon glove = newEquip as Weapon;
                    itemCapsule.thisItem = glove;
                    inventoryManager._WeaponsInBag.Add(itemCapsule);
                    break;
                #endregion
                #region Grimoire
                case string a when a.Contains("Grimoire"):
                    newEquip = gameManager._GrimoiresDatabase.Find(x => x.guid == SD.inventorySaveData.EquipmentIdData[i]);

                    itemCapsule.thisItem = newEquip;
                    itemCapsule.ItemAmount = SD.inventorySaveData.EquipmentAmountData[i];
                    inventoryManager.EquipmentInBag.Add(itemCapsule);

                    Weapon grimoire = newEquip as Weapon;
                    itemCapsule.thisItem = grimoire;
                    inventoryManager._WeaponsInBag.Add(itemCapsule);
                    break;
                #endregion
                #endregion
                #region ARMOUR DATA
                #region Leather
                case string a when a.Contains("Leather"):
                    newEquip = gameManager._LeatherDatabase.Find(x => x.guid == SD.inventorySaveData.EquipmentIdData[i]);

                    itemCapsule.thisItem = newEquip;
                    itemCapsule.ItemAmount = SD.inventorySaveData.EquipmentAmountData[i];
                    inventoryManager.EquipmentInBag.Add(itemCapsule);

                    Armour arm = newEquip as Armour;
                    itemCapsule.thisItem = arm;
                    inventoryManager._ArmourInBag.Add(itemCapsule);
                    break;
                #endregion
                #region Mail
                case string a when a.Contains("Mail"):
                    newEquip = gameManager._MailDatabase.Find(x => x.guid == SD.inventorySaveData.EquipmentIdData[i]);

                    itemCapsule.thisItem = newEquip;
                    itemCapsule.ItemAmount = SD.inventorySaveData.EquipmentAmountData[i];
                    inventoryManager.EquipmentInBag.Add(itemCapsule);

                    Armour mail = newEquip as Armour;
                    itemCapsule.thisItem = mail;
                    inventoryManager.EquipmentInBag.Add(itemCapsule);
                    break;
                #endregion
                #region Chasis
                case string a when a.Contains("Chasis"):
                    newEquip = gameManager._ChasisDatabase.Find(x => x.guid == SD.inventorySaveData.EquipmentIdData[i]);

                    itemCapsule.thisItem = newEquip;
                    itemCapsule.ItemAmount = SD.inventorySaveData.EquipmentAmountData[i];
                    inventoryManager.EquipmentInBag.Add(itemCapsule);

                    Armour chasis = newEquip as Armour;
                    itemCapsule.thisItem = chasis;
                    inventoryManager._ArmourInBag.Add(itemCapsule);
                    break;
                #endregion
                #region Robe
                case string a when a.Contains("Robe"):
                    newEquip = gameManager._RobesDatabase.Find(x => x.guid == SD.inventorySaveData.EquipmentIdData[i]);

                    itemCapsule.thisItem = newEquip;
                    itemCapsule.ItemAmount = SD.inventorySaveData.EquipmentAmountData[i];
                    inventoryManager.EquipmentInBag.Add(itemCapsule);

                    Armour robe = newEquip as Armour;
                    itemCapsule.thisItem = robe;
                    inventoryManager._ArmourInBag.Add(itemCapsule);
                    break;
                #endregion
                #endregion
                #region ACCESSORY DATA
                case string a when a.Contains("Accessory"):
                    newEquip = gameManager._AccessoryDatabase.Find(x => x.guid == SD.inventorySaveData.EquipmentIdData[i]);

                    itemCapsule.thisItem = newEquip;
                    itemCapsule.ItemAmount = SD.inventorySaveData.EquipmentAmountData[i];
                    inventoryManager.EquipmentInBag.Add(itemCapsule);

                    Accessory accessory = newEquip as Accessory;
                    itemCapsule.thisItem = accessory;
                    inventoryManager._AccessoryInBag.Add(itemCapsule);
                    break;
                #endregion
            }
        }
        // Add Loot into Bag
        for (int i = 0; i < SD.inventorySaveData.LootIdData.Count; i++)
        {
            Loot newLoot = gameManager._LootDatabase.Find(x => x.guid == SD.inventorySaveData.KeyItemsIdData[i]);
            ItemCapsule itemCapsule = new();
            itemCapsule.thisItem = newLoot;
            itemCapsule.ItemAmount = SD.inventorySaveData.LootAmountData[i];
            inventoryManager.LootInBag.Add(itemCapsule);
        }
        // Add Key Items into Bag
        for (int i = 0; i < SD.inventorySaveData.KeyItemsIdData.Count; i++)
        {
            KeyItem newKey = gameManager._KeyItemsDatabase.Find(x => x.guid == SD.inventorySaveData.KeyItemsIdData[i]);
            ItemCapsule itemCapsule = new();
            itemCapsule.thisItem = newKey;
            itemCapsule.ItemAmount = 1;
            inventoryManager.KeyItemsInBag.Add(itemCapsule);
        }
        // Add Money into Bag
        inventoryManager.creditsInBag = SD.inventorySaveData.CreditsAmountData;
        #endregion
        #region Time Data
        FindObjectOfType<InGameClock>().playTime = SD.playTimeData;
        #endregion
        LoadGameScene(gameManager.LastKnownScene);
    }
    private void LoadGameScene(string SceneToLoad)
    {
        SceneManager.LoadScene(SceneToLoad);
    }
}
