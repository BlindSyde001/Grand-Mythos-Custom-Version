using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

public class MenuInputs : MonoBehaviour
{
    // VARIABLES
    public GameObject LoadList;
    public List<SaveFileButton> SavedFiles;

    [SerializeField]
    private int currentMenuOpen = 10;
    internal float speed = 0.5f;
    private bool coroutineRunning = false;
    private bool closeAllOverride = false;

    private int heroLineupChangeOne;
    private int heroLineupChangeTwo;
    #region Menu Lists
    [SerializeField]
    private List<PartyContainer> displayList;
    [SerializeField]
    private List<PartyContainer> reservesList;
    [SerializeField]
    private MiscContainer miscList;
    #endregion

    // METHODS
    #region File IO
    public void OpenLoadFiles(int openType)
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

            #region Change the button Actions Based on whether you're Saving or Loading
            SavedFiles[i].GetComponent<Button>().onClick.RemoveAllListeners();
            int q = i;
            switch (openType)
            {
                case 0:
                    SavedFiles[i].GetComponent<Button>().onClick.AddListener(delegate { ClickSave(q); });
                    break;

                case 1:
                    SavedFiles[i].GetComponent<Button>().onClick.AddListener(delegate { ClickLoad(q); });
                    break;
            }
            #endregion
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
        // Find and save Positional Data
        GameManager._instance._LastKnownScene = SceneManager.GetActiveScene().name;
        GameManager._instance._LastKnownPosition = FindObjectOfType<OverworldPlayerCircuit>().transform.position;
        GameManager._instance._LastKnownRotation = FindObjectOfType<OverworldPlayerCircuit>().transform.rotation;

        SaveData.current.savedScene = GameManager._instance._LastKnownScene;
        SaveData.current.overworldPos = GameManager._instance._LastKnownPosition;
        SaveData.current.overworldRot = GameManager._instance._LastKnownRotation;

        // Set x as Index Number of Hero in AllList, so that you can pull that hero by Index when making the Lineup
        SaveData.current.lineupSave.Clear();
        foreach(HeroExtension hero in GameManager._instance._PartyLineup)
        {
            int x = GameManager._instance._AllPartyMembers.IndexOf(hero);
            SaveData.current.lineupSave.Add(x);
        }

        // Creating Data packets per hero and saving those
        SaveData.current.heroSaveData.Clear();
        foreach(HeroExtension hero in GameManager._instance._AllPartyMembers)
        {
            SerializableHero heroSave = new SerializableHero
            {
                totalExperienceSave = hero._TotalExperience,

                weaponIDSave = hero._Weapon._ItemID,
                armourIDSave = hero._Armour._ItemID,
                accessoryOneIDSave = hero._AccessoryOne._ItemID,
                accessoryTwoIDSave = hero._AccessoryTwo._ItemID,

                weaponSave = hero._Weapon.weaponType.ToString(),
                armourSave = hero._Armour.armourType.ToString(),

            };
            Debug.Log(hero.name + ": Weapon ID is: " + heroSave.weaponIDSave);

            SaveData.current.heroSaveData.Add(heroSave);
        }

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
    #endregion

    public void MenuSwitchboard(int newMenuToOpen)
    {
        if (currentMenuOpen == newMenuToOpen)
        {
            return;
        }
        CloseSwitchBoard(currentMenuOpen);
        switch(newMenuToOpen)
        {
            case 0:         // Start
                StartMenuOpen();
                DisplayPartyHeroes();
                DisplayReserves();
                break;
            case 1:         // Item
                break;
            case 2:         // Abilities
                break;
            case 3:         // Equipment
                break;
            case 4:         // Status
                break;
            case 5:         // Tactics
                break;
            case 6:         // Journal
                break;
            case 7:         // Save
                SaveMenuOpen();
                break;
            case 8:         // Settings
                break;
            case 9:         // Quit
                break;
        }
        currentMenuOpen = newMenuToOpen;
    }
    public void CloseSwitchBoard(int menuToClose)
    {
        switch (menuToClose)
        {
            case 0:
                StartCoroutine(StartMenuClose());
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                break;
            case 7:
                StartCoroutine(SaveMenuClose(closeAllOverride));
                break;
            case 8:
                break;
            case 9:
                break;
        }
    }

    public IEnumerator OpenFirstMenu()
    {
        if (!coroutineRunning)
        {
            coroutineRunning = true;
            MenuSwitchboard(0);
            InputManager._instance.MenuGraphic.GetComponent<Image>().DOFade(1, InputManager._instance.menuInputs.speed);
            yield return new WaitForSeconds(speed);
            coroutineRunning = false;
            EventManager._instance.SwitchGameState(GameState.MENU);
        }
    }
    public IEnumerator CloseAllMenus()
    {
        closeAllOverride = true;
        if (!coroutineRunning)
        {
            InputManager._instance.MenuGraphic.GetComponent<Image>().DOFade(0, InputManager._instance.menuInputs.speed);
            CloseSwitchBoard(currentMenuOpen);
            yield return new WaitForSeconds(speed);
            currentMenuOpen = 10;
            EventManager._instance.SwitchGameState(GameState.OVERWORLD);
        }
        closeAllOverride = false;
    }


    public void ChangePartyLineup(int selectedToChange)
    {
        if(heroLineupChangeOne == 0)
        {
            heroLineupChangeOne = selectedToChange;
            Debug.Log("Set 1!");
        }
        else if(heroLineupChangeTwo == 0)
        {
            heroLineupChangeTwo = selectedToChange;
            Debug.Log("Set 2!");
            PerformSwap(heroLineupChangeOne, heroLineupChangeTwo);
        }
    }
    private void PerformSwap(int heroOne, int heroTwo)
    {
        HeroExtension a = GameManager._instance._PartyLineup[heroOne - 1];
        HeroExtension b = GameManager._instance._PartyLineup[heroTwo - 1];

        GameManager._instance._PartyLineup[heroOne - 1] = b;
        GameManager._instance._PartyLineup[heroTwo - 1] = a;
        DisplayPartyHeroes();
        Debug.Log("It worked!");

        heroLineupChangeOne = 0;
        heroLineupChangeTwo = 0;
    }
    #region Start Menu
    private void StartMenuOpen()
    {
        foreach (GameObject a in InputManager._instance.MenuItems)
        {
            a.SetActive(false);
        }
        InputManager._instance.MenuItems[0].SetActive(true);
        InputManager._instance.MenuItems[0].transform.GetChild(0).DOLocalMove(new Vector3(-740, 120, 0), speed);
        InputManager._instance.MenuItems[0].transform.GetChild(1).DOLocalMove(new Vector3(-740, -290, 0), speed);
        InputManager._instance.MenuItems[0].transform.GetChild(2).DOLocalMove(new Vector3(200, 125, 0), speed);
        InputManager._instance.MenuItems[0].transform.GetChild(3).DOLocalMove(new Vector3(200, -400, 0), speed);
    }
    private IEnumerator StartMenuClose()
    {
        if (!coroutineRunning)
        {
            InputManager._instance.MenuItems[0].transform.GetChild(0).DOLocalMove(new Vector3(-1200, 120, 0), speed);
            InputManager._instance.MenuItems[0].transform.GetChild(1).DOLocalMove(new Vector3(-1200, -290, 0), speed);
            InputManager._instance.MenuItems[0].transform.GetChild(2).DOLocalMove(new Vector3(1700, 125, 0), speed);
            InputManager._instance.MenuItems[0].transform.GetChild(3).DOLocalMove(new Vector3(1700, -400, 0), speed);
            yield return new WaitForSeconds(speed);
            InputManager._instance.MenuItems[0].SetActive(false);
        }
    }
    private void DisplayPartyHeroes()
    {
        foreach(PartyContainer a in displayList)
        {
            a.gameObject.SetActive(false);
        }
        for(int i = 0; i < GameManager._instance._PartyLineup.Count; i++)
        {
            displayList[i].gameObject.SetActive(true);
            displayList[i].displayName.text = GameManager._instance._PartyLineup[i].charName;
           
            displayList[i].displayBanner.sprite = GameManager._instance._PartyLineup[i].charBanner;
           
            displayList[i].displayLevel.text = GameManager._instance._PartyLineup[i]._Level.ToString();
           
            displayList[i].displayEXPBar.value =
                (float)GameManager._instance._PartyLineup[i]._TotalExperience / 
                GameManager._instance._PartyLineup[i].ExperienceThreshold;

            displayList[i].displayHP.text =
                "HP: " + GameManager._instance._PartyLineup[i]._CurrentHP.ToString() + " / " +
                GameManager._instance._PartyLineup[i].MaxHP.ToString();

            displayList[i].displayMP.text =
                "MP: " + GameManager._instance._PartyLineup[i]._CurrentMP.ToString() + " / " +
                GameManager._instance._PartyLineup[i].MaxMP.ToString();
        }
    }
    private void DisplayReserves()
    {
        foreach (PartyContainer a in reservesList)
        {
            a.gameObject.SetActive(false);
        }
        for (int i = 0; i < GameManager._instance._ReservesLineup.Count; i++)
        {
            reservesList[i].displayName.text = GameManager._instance._PartyLineup[i].charName;

            reservesList[i].displayLevel.text = GameManager._instance._PartyLineup[i]._Level.ToString();

            reservesList[i].displayEXPBar.value =
                (float)GameManager._instance._PartyLineup[i]._TotalExperience /
                GameManager._instance._PartyLineup[i].ExperienceThreshold;

            reservesList[i].displayHP.text =
                "HP: " + GameManager._instance._PartyLineup[i]._CurrentHP.ToString() + " / " +
                GameManager._instance._PartyLineup[i].MaxHP.ToString();

            reservesList[i].displayMP.text =
                "MP: " + GameManager._instance._PartyLineup[i]._CurrentMP.ToString() + " / " +
                GameManager._instance._PartyLineup[i].MaxMP.ToString();
        }
    }
    private void DisplayMisc()
    {

    }
    #endregion
    #region Item Menu
    #endregion
    #region Abilities Menu
    #endregion
    #region Equipment Menu
    #endregion
    #region Status Menu
    #endregion
    #region Tactics Menu
    #endregion
    #region Journal Menu
    #endregion
    #region Save Menu
    private void SaveMenuOpen()
    {
        InputManager._instance.MenuItems[7].SetActive(true);
        InputManager._instance.MenuItems[7].transform.GetChild(0).DOLocalMove(new Vector3(190, 0, 0), speed);
        InputManager._instance.MenuItems[7].transform.GetChild(1).DOLocalMove(new Vector3(-755, 465, 0), speed);
        OpenLoadFiles(0);
    }
    private IEnumerator SaveMenuClose(bool closeAllOverride)
    {
        if (!coroutineRunning)
        {
            coroutineRunning = true;
            InputManager._instance.MenuItems[7].transform.GetChild(0).DOLocalMove(new Vector3(1750, 0, 0), speed);
            InputManager._instance.MenuItems[7].transform.GetChild(1).DOLocalMove(new Vector3(-1200, 465, 0), speed);
            yield return new WaitForSeconds(speed);
            InputManager._instance.MenuItems[7].SetActive(false);
            coroutineRunning = false;
        }
        if(!closeAllOverride)
        {
            StartMenuOpen();
            yield return new WaitForSeconds(speed);
            currentMenuOpen = 0;
        }
    }
    #endregion
    #region Settings Menu
    #endregion
    #region Quit Menu
    private void QuitApplication()
    {
        Application.Quit();
    }
    #endregion
}
