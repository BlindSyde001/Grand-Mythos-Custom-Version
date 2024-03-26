using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class SaveMenuActions : MenuContainer
{
    public UIElementList<SaveFileButton> SaveFileButtons = new();
    public UnityEvent OnSave;

    // METHODS
    public void OpenLoadFiles()
    {
        SavingSystem.FeedFileUI(SaveFileButtons, file =>
        {
            if (SavingSystem.TrySaveToDisk(file))
            {
                OpenLoadFiles();
                OnSave?.Invoke();
            }
        });
    }

    public void SaveToNewFile() => SavingSystem.TrySaveToDisk();

    public override IEnumerable Open(MenuInputs menuInputs)
    {
        gameObject.SetActive(true);
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-800, gameObject.transform.GetChild(0).transform.localPosition.y, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(-800, gameObject.transform.GetChild(1).transform.localPosition.y, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(190, gameObject.transform.GetChild(2).transform.localPosition.y, 0), menuInputs.Speed);
        OpenLoadFiles();
        yield return new WaitForSeconds(menuInputs.Speed);
    }
    public override IEnumerable Close(MenuInputs menuInputs)
    {
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-1200, gameObject.transform.GetChild(0).transform.localPosition.y, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(-1200, gameObject.transform.GetChild(1).transform.localPosition.y, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(1750, gameObject.transform.GetChild(2).transform.localPosition.y, 0), menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
        gameObject.SetActive(false);
    }
}