using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class SaveMenuActions : MenuContainer
{
    public List<SaveFileButton> SavedFiles;
    public TextMeshProUGUI savedtext;

    // METHODS
    public void OpenLoadFiles()
    {
        SavingSystem.FeedFileUI(SavedFiles, file =>
        {
            if (SavingSystem.TrySaveToDisk(file))
                StartCoroutine(SavedGameGraphic());
        });
    }

    public void SaveToNewFile() => SavingSystem.TrySaveToDisk();

    IEnumerator SavedGameGraphic()
    {
        savedtext.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        savedtext.gameObject.SetActive(false);
        OpenLoadFiles();
    }

    public override IEnumerable Open(MenuInputs menuInputs)
    {
        gameObject.SetActive(true);
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(-800, gameObject.transform.GetChild(1).transform.localPosition.y, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(190, 0, 0), menuInputs.Speed);
        OpenLoadFiles();
        yield return new WaitForSeconds(menuInputs.Speed);
    }
    public override IEnumerable Close(MenuInputs menuInputs)
    {
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-1200, 480, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(-1200, gameObject.transform.GetChild(1).transform.localPosition.y, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(1750, 0, 0), menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
        gameObject.SetActive(false);
    }
}