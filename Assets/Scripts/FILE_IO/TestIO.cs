using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TestIO : MonoBehaviour
{
    // VARIABLES
    public GameObject LoadList;
    public List<GameObject> SavedFiles;

    // METHODS
    public void ClickSave(int SaveFileNumber)
    {
        // Find and save Positional Data
        GameManager._instance._LastKnownScene = SceneManager.GetActiveScene().name;
        GameManager._instance._LastKnownPosition = FindObjectOfType<OverworldPlayerCircuit>().transform.position;
        GameManager._instance._LastKnownRotation = FindObjectOfType<OverworldPlayerCircuit>().transform.rotation;

        SaveData.current.savedScene = GameManager._instance._LastKnownScene;
        SaveData.current.overworldPos = GameManager._instance._LastKnownPosition;
        SaveData.current.overworldRot = GameManager._instance._LastKnownRotation;

        // Set x as Index Number of Hero in AllList, so that you can pull that hero by Index when making the Lineup
        foreach(HeroExtension hero in GameManager._instance._PartyLineup)
        {
            int x = GameManager._instance._AllPartyMembers.IndexOf(hero);
            SaveData.current.lineupSave.Add(x);
        }
        // Creating Data packets per hero and saving those
        foreach(HeroExtension hero in GameManager._instance._AllPartyMembers)
        {
            SerializableHero heroSave = new SerializableHero
            {
                totalExperienceSave = hero._TotalExperience,

                weaponIDSave = hero._Weapon._ItemID,
                armourIDSave = hero._Armour._ItemID,
                accessoryOneIDSave = hero._AccessoryOne._ItemID,
                accessoryTwoIDSave = hero._AccessoryTwo._ItemID,
            };

            SaveData.current.heroSaveData.Add(heroSave);
        }

        Debug.Log(SaveFileNumber + "B4 Sending");
        SaveManager.SaveToFile(SaveData.current, SaveFileNumber);
        Debug.Log("Saved"); 
    }
    public void ClickLoad(int FileNumber)
    {
       SaveData SD = SaveManager.LoadFromFile(FileNumber);

        GameManager._instance._LastKnownScene = SD.savedScene;
        GameManager._instance._LastKnownPosition = SD.overworldPos;
        GameManager._instance._LastKnownRotation = SD.overworldRot;

        for(int i = 0; i < GameManager._instance._AllPartyMembers.Count; i++)
        {
            GameManager._instance._AllPartyMembers[i]._TotalExperience = SD.heroSaveData[i].totalExperienceSave;
            SaveManager.ExtractWeaponData(SD, i);
            SaveManager.ExtractArmourData(SD, i);
            SaveManager.ExtractAccessoryData(SD, i);
        }
        
        GameManager._instance._PartyLineup.Clear();
        for(int i = 0; i < SD.lineupSave.Count; i++)
        {
            GameManager._instance._PartyLineup.Add(GameManager._instance._AllPartyMembers[i]);
        }
    }

    public void OpenLoadFiles()
    {
        // read the data on files
        string[] readFiles = GetFileNames(Application.persistentDataPath + "/Save Files", "*.json");
        foreach (string name in readFiles)
        {
            Debug.Log(name);
        }
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
                    tempHero.Add(GameManager._instance._AllPartyMembers[SD.lineupSave[j]]);
                }

                // foreach party member, display their icon in order
                for (int k = 0; k < SD.lineupSave.Count; k++)
                {
                    SavedFiles[i].transform.Find("Party List").GetChild(k).GetComponent<Image>().sprite =
                        tempHero[k].charPortrait;
                }
            }
            // Display name of the file
            SavedFiles[i].transform.Find("File Name").GetComponent<TextMeshProUGUI>().text =
                readFiles[i];

            // Change button Action
            SavedFiles[i].GetComponent<Button>().onClick.AddListener(delegate { ClickSave(i); });
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
}
