using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class TitleScreenIO : MonoBehaviour
{
    public SpawnPointReference NewGameScene;
    [FormerlySerializedAs("SavedFiles")] public List<SaveFileButton> SavedFilesUI;

    // METHODS
    public void OpenLoadFiles()
    {
        SavingSystem.FeedFileUI(SavedFilesUI, file => SavingSystem.TryLoadFromDisk(file));
    }

    public void NewGame()
    {
        NewGameScene.SwapSceneToThisSpawnPoint();
    }
}
