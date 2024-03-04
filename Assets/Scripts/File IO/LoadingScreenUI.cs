using System.Collections.Generic;
using UnityEngine;

public class LoadingScreenUI : MonoBehaviour
{
    public List<SaveFileButton> SavedFilesUI;

    public void OnEnable() => Refresh();
    public void Refresh() => SavingSystem.FeedFileUI(SavedFilesUI, file => SavingSystem.TryLoadFromDisk(file));
}