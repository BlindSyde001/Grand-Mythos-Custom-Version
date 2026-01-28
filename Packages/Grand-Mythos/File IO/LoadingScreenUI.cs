using UnityEngine;

public class LoadingScreenUI : MonoBehaviour
{
    public UIElementList<SaveFileButton> SavedFilesUI = new(){ Template = null! };

    public void OnEnable() => Refresh();
    public void Refresh() => SavingSystem.FeedFileUI(SavedFilesUI, file => SavingSystem.TryLoadFromDisk(file));
}