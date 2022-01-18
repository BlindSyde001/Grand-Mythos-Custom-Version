using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class TitleScreenIO : MonoBehaviour
{
    // VARIABLES
    public string NewGameScene;
    public GameObject LoadList;
    public List<SaveFileButton> SavedFiles;

    // METHODS
    public void OpenLoadFiles()
    {
        string[] readFiles =  GetFileNames(Application.persistentDataPath + "/Save Files", "*.json");
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
                    SavedFiles[i].characterPortraits[k].sprite = tempHero[k].charPortrait;
                }
            }
            // Display name of the file
            SavedFiles[i].fileName.text = "Saved Game " + i;
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

    public void NewGame()
    {
        SceneChangeManager._instance.LoadNewZone(NewGameScene);
    }   
    public void LoadGameData(int FileNumber)
    {
        var SD = SaveManager.LoadFromFile(FileNumber);

        GameManager._instance._LastKnownScene = SD.savedScene;
        GameManager._instance._LastKnownPosition = SD.overworldPos;
        GameManager._instance._LastKnownRotation = SD.overworldRot;


        for (int i = 0; i < GameManager._instance._AllPartyMembers.Count; i++)
        {
            GameManager._instance._AllPartyMembers[i]._TotalExperience = SD.heroSaveData[i].totalExperienceSave;
            GameManager._instance._AllPartyMembers[i].LevelUpCheck();
            SaveManager.ExtractWeaponData(SD, i);
            SaveManager.ExtractArmourData(SD, i);
            SaveManager.ExtractAccessoryData(SD, i);
        }

        GameManager._instance._PartyLineup.Clear();
        for (int i = 0; i < SD.lineupSave.Count; i++)
        {
            GameManager._instance._PartyLineup.Add(GameManager._instance._AllPartyMembers[SD.lineupSave[i]]);
        }
        LoadGameScene(GameManager._instance._LastKnownScene);
    }
    private void LoadGameScene(string SceneToLoad)
    {
        SceneManager.LoadScene(SceneToLoad);
    }
}
