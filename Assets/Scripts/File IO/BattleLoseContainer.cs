using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class BattleLoseContainer : MonoBehaviour
{
    public List<SaveFileButton> SavedFiles;

    // METHODS
    public void QuitGame()
    {
        Application.Quit();
    }
    public void OpenLoadFiles()
    {
        SavingSystem.FeedFileUI(SavedFiles, saveName => SavingSystem.TryLoadFromDisk(saveName));
    }
}
