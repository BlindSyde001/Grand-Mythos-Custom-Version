using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenIO : MonoBehaviour
{
    public GameObject LoadList;
    public List<GameObject> SavedFiles;

    public void NewGame()
    {
        SceneManager.LoadScene(2);
    }   
    public void LoadGame(string fileName)
    {
        var SD = SaveManager.LoadFromFile(fileName);

        GameManager._instance._LastKnownScene = SD.scene;
        GameManager._instance._LastKnownPosition = SD.overworldPos;
        GameManager._instance._LastKnownRotation = SD.overworldRot;

        GameManager._instance._AllPartyMembers = SD.allPartyMembersSave;
        GameManager._instance._PartyLineup = SD.partyLineupSave;

        for (int i = 0; i < GameManager._instance._AllPartyMembers.Count; i++)
        {
            GameManager._instance._AllPartyMembers[i]._TotalExperience = SD.heroSaveData[i].totalExperienceSave;
            SaveManager.ExtractWeaponData(SD, i);
            SaveManager.ExtractArmourData(SD, i);
            SaveManager.ExtractAccessoryData(SD, i);
        }
    }


    public void OpenLoadFiles()
    {
        foreach(GameObject button in SavedFiles)
        {
            button.SetActive(false);
        }

        string[] readFiles =  GetFileNames(Application.persistentDataPath + "/Save Files", "*.json");
        foreach(string a in readFiles)
        {
            Debug.Log(a);
        }

        for (int i = 0; i < readFiles.Length; i++)
        {
            var SD = SaveManager.LoadFromFile(readFiles[i]);

            SavedFiles[i].SetActive(true);

            for (int j = 0; j < SD.partyLineupSave.Count; j++)
            {
                SavedFiles[i].transform.Find("Party List").GetChild(j).GetComponent<Image>().sprite =
                    SD.partyLineupSave[j].charPortrait;
            }
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
