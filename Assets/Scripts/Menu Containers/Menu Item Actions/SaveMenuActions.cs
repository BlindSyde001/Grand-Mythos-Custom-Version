using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;

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
        if (!menuInputs.coroutineRunning)
        {
            yield return new WaitForSeconds(menuInputs.speed);
            gameObject.SetActive(true);
            gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.speed);
            gameObject.transform.GetChild(1).DOLocalMove(new Vector3(-800, gameObject.transform.GetChild(1).transform.localPosition.y, 0), menuInputs.speed);
            gameObject.transform.GetChild(2).DOLocalMove(new Vector3(190, 0, 0), menuInputs.speed);
            OpenLoadFiles();
        }
    }
    public override IEnumerable Close(MenuInputs menuInputs)
    {
        if (!menuInputs.coroutineRunning)
        {
            menuInputs.coroutineRunning = true;
            gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-1200, 480, 0), menuInputs.speed);
            gameObject.transform.GetChild(1).DOLocalMove(new Vector3(-1200, gameObject.transform.GetChild(1).transform.localPosition.y, 0), menuInputs.speed);
            gameObject.transform.GetChild(2).DOLocalMove(new Vector3(1750, 0, 0), menuInputs.speed);
            yield return new WaitForSeconds(menuInputs.speed);
            gameObject.SetActive(false);
            menuInputs.coroutineRunning = false;
        }
    }
}