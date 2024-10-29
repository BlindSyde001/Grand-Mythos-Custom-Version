using UnityEngine;

public class LoadingScreenUI : MonoBehaviour
{
    public UIElementList<SaveFileButton> SavedFilesUI = new();

    public void OnEnable() => Refresh();
    public void Refresh() => SavingSystem.FeedFileUI(SavedFilesUI, file => SavingSystem.TryLoadFromDisk(file));
}