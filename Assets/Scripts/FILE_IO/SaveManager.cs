using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEngine;

public static class SaveManager
{
    public static void SaveToFile(SaveData saveData, string newFileName)
    {
        if(!Directory.Exists(Application.persistentDataPath + "/Save Files")) // Creates the Folder for Save files if there isn't one already
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Save Files");
        }

        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(Application.persistentDataPath + "/Save Files/" + newFileName + ".json", json);
    }
    public static SaveData LoadFromFile(string fileName)
    {
        string json = "";
        SaveData loadData = new SaveData();
        try
        {
            json = File.ReadAllText(Application.persistentDataPath + "/Save Files/" + fileName);
            loadData = JsonUtility.FromJson<SaveData>(json);
        }
        catch
        {
            Debug.LogError("Warning, could not load the file!");
        }
        return loadData;
    }



    internal static void ExtractWeaponData(SaveData SD, int i)
    {
        if (SD.heroSaveData[i].weaponSave != null)
        {
            switch (SD.heroSaveData[i].weaponSave.weaponType)
            {
                case Weapon.WeaponType.Gun:
                    try
                    {
                        GameManager._instance._AllPartyMembers[i]._Weapon = GameManager._instance._GunsDatabase.Find(x => x._ItemID == SD.heroSaveData[i].weaponIDSave);
                    }
                    catch
                    {
                        Debug.LogError("Cannot find Gun in the Database!");
                    }
                    break;
                case Weapon.WeaponType.Warhammer:
                    try
                    {
                        GameManager._instance._AllPartyMembers[i]._Weapon = GameManager._instance._WarhammersDatabase.Find(x => x._ItemID == SD.heroSaveData[i].weaponIDSave);
                    }
                    catch
                    {
                        Debug.LogError("Cannot find Warhammer in the Database!");
                    }
                    break;
                case Weapon.WeaponType.PowerGlove:
                    try
                    {
                        GameManager._instance._AllPartyMembers[i]._Weapon = GameManager._instance._PowerGlovesDatabase.Find(x => x._ItemID == SD.heroSaveData[i].weaponIDSave);
                    }
                    catch
                    {
                        Debug.LogError("Cannot find Power Glove in the Database!");
                    }
                    break;
                case Weapon.WeaponType.Grimoire:
                    try
                    {
                        GameManager._instance._AllPartyMembers[i]._Weapon = GameManager._instance._GrimoiresDatabase.Find(x => x._ItemID == SD.heroSaveData[i].weaponIDSave);
                    }
                    catch
                    {
                        Debug.LogError("Cannot find Grimoire in the Database!");
                    }
                    break;
            }
        }
        else
        {
            Debug.LogError("There is no Weapon equipped! Character always must have a Weapon");
        }
    }
    internal static void ExtractArmourData(SaveData SD, int i)
    {
        if (SD.heroSaveData[i].armourSave != null)
        {
            switch (SD.heroSaveData[i].armourSave.armourType)
            {
                case Armour.ArmourType.Leather:
                    try
                    {
                        GameManager._instance._AllPartyMembers[i]._Armour = GameManager._instance._LeatherDatabase.Find(x => x._ItemID == SD.heroSaveData[i].armourIDSave);
                    }
                    catch
                    {
                        Debug.LogError("Cannot find Leather in the Database!");
                    }
                    break;
                case Armour.ArmourType.Mail:
                    try
                    {
                        GameManager._instance._AllPartyMembers[i]._Armour = GameManager._instance._MailDatabase.Find(x => x._ItemID == SD.heroSaveData[i].armourIDSave);
                    }
                    catch
                    {
                        Debug.LogError("Cannot find Mail in the Database!");
                    }
                    break;
                case Armour.ArmourType.Chasis:
                    try
                    {
                        GameManager._instance._AllPartyMembers[i]._Armour = GameManager._instance._ChasisDatabase.Find(x => x._ItemID == SD.heroSaveData[i].armourIDSave);
                    }
                    catch
                    {
                        Debug.LogError("Cannot find Chasis in the Database!");
                    }
                    break;
                case Armour.ArmourType.Robes:
                    try
                    {
                        GameManager._instance._AllPartyMembers[i]._Armour = GameManager._instance._RobesDatabase.Find(x => x._ItemID == SD.heroSaveData[i].armourIDSave);
                    }
                    catch
                    {
                        Debug.LogError("Cannot find Robe in the Database!");
                    }
                    break;
            }
        }
        else
        {
            Debug.LogError("There is no Armour equipped! Character always must have a Weapon");
        }
    }
    internal static void ExtractAccessoryData(SaveData SD, int i)
    {
        if (SD.heroSaveData[i].accessoryOneSave != null)
        {
            try
            {
                GameManager._instance._AllPartyMembers[i]._AccessoryOne = GameManager._instance._AccessoryDatabase.Find(x => x._ItemID == SD.heroSaveData[i].accessoryOneIDSave);
            }
            catch
            {
                Debug.LogError("Cannot find Accessory for Acc1 in the Database!");
            }
        }
        if (SD.heroSaveData[i].accessoryTwoSave != null)
        {
            try
            {
                GameManager._instance._AllPartyMembers[i]._AccessoryTwo = GameManager._instance._AccessoryDatabase.Find(x => x._ItemID == SD.heroSaveData[i].accessoryTwoIDSave);
            }
            catch
            {
                Debug.LogError("Cannot find Accessory for Acc2 in the Database!");
            }
        }
    }
}
