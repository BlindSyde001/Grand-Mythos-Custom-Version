using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.UI;

public class SaveMenuActions : MenuContainer
{
    public UIElementList<SaveFileButton> SaveFileButtons = new(){ Template = null! };
    public UnityEvent? OnSave;
    public required GameObject OverwritePanel;
    public required Button OverwriteYes, OverwriteNo;

    // METHODS
    public void OpenLoadFiles()
    {
        SavingSystem.FeedFileUI(SaveFileButtons, file =>
        {
            OverwritePanel.SetActive(true);
            OverwriteYes.onClick.RemoveAllListeners();
            OverwriteNo.onClick.RemoveAllListeners();

            OverwriteYes.onClick.AddListener(() =>
            {
                OverwritePanel.SetActive(false);
                if (SavingSystem.TrySaveToDisk(file))
                {
                    OpenLoadFiles();
                    OnSave?.Invoke();
                }
            });
            OverwriteNo.onClick.AddListener(() =>
            {
                OverwritePanel.SetActive(false);
            });
        });
    }

    public void SaveToNewFile() => SavingSystem.TrySaveToDisk();

    public override IEnumerable Open(MenuInputs menuInputs)
    {
        OpenLoadFiles();
        gameObject.SetActive(true);
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-800, gameObject.transform.GetChild(0).localPosition.y, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(190, gameObject.transform.GetChild(1).localPosition.y, 0), menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
    }
    public override IEnumerable Close(MenuInputs menuInputs)
    {
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-1200, gameObject.transform.GetChild(0).localPosition.y, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(1750, gameObject.transform.GetChild(1).localPosition.y, 0), menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
        gameObject.SetActive(false);
    }
}