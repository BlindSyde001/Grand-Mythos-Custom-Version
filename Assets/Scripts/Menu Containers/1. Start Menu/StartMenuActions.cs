using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StartMenuActions : MenuContainer
{
    [SerializeField]
    private List<PartyContainer> displayList;
    [SerializeField]
    private List<ReserveContainer> reserveDisplayList;
    [SerializeField]
    private MiscContainer miscList;
    [SerializeField]
    private InGameClock inGameClock;

    private void LateUpdate()
    {
        miscList.miscTime.text = ((inGameClock.hour < 10)? ("0" + inGameClock.hour) : inGameClock.hour) + ":" + 
                                 ((inGameClock.minute < 10)? ("0" + inGameClock.minute) : inGameClock.minute) + ":" + 
                                 ((inGameClock.second < 10)? ("0" + inGameClock.second) : inGameClock.second); 
    }
    // METHODS
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        foreach (MenuContainer a in menuInputs.GetComponentsInChildren<MenuContainer>())
        {
            if (a == this)
                continue;
            a.gameObject.SetActive(false);
        }
        gameObject.SetActive(true);
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-740, 150, 0), menuInputs.speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(200, 30, 0), menuInputs.speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(200, gameObject.transform.GetChild(2).localPosition.y, 0), menuInputs.speed);
        gameObject.transform.GetChild(3).DOLocalMove(new Vector3(0, 0, 0), menuInputs.speed);
        DisplayPartyHeroes();
        DisplayMisc();
        yield break;
    }
    public override IEnumerable Close(MenuInputs menuInputs)
    {
        if (!menuInputs.coroutineRunning)
        {
            gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-1200, 150, 0), menuInputs.speed);
            gameObject.transform.GetChild(1).DOLocalMove(new Vector3(1700, 30, 0), menuInputs.speed);
            gameObject.transform.GetChild(2).DOLocalMove(new Vector3(1700, gameObject.transform.GetChild(2).localPosition.y, 0), menuInputs.speed);
            gameObject.transform.GetChild(3).DOLocalMove(new Vector3(0, -150, 0), menuInputs.speed);
            yield return new WaitForSeconds(menuInputs.speed);
            gameObject.SetActive(false);
        }
    }
    internal void DisplayPartyHeroes()
    {
        OnEnable(); // Ensure managers are up to date
        foreach (PartyContainer a in displayList)
        {
            a.gameObject.SetActive(false);
        }
        for (int i = 0; i < GameManager._PartyLineup.Count; i++)
        {
            var hero = GameManager._PartyLineup[i];
            displayList[i].gameObject.SetActive(true);
            displayList[i].displayName.text = hero.charName;
            displayList[i].displayBanner.sprite = hero.charBanner;
            displayList[i].displayLevel.text = hero.Level.ToString();

            displayList[i].displayEXPBar.fillAmount = (float)(hero._TotalExperience - hero.PrevExperienceThreshold) /
                                                             (hero.ExperienceThreshold - hero.PrevExperienceThreshold);

            displayList[i].displayHP.text =
                hero._CurrentHP.ToString() + " / " +
                hero.MaxHP.ToString();
        }

        foreach (ReserveContainer a in reserveDisplayList)
        {
            a.gameObject.SetActive(false);
        }
        for (int i = 0; i < GameManager._ReservesLineup.Count; i++)
        {
            var hero = GameManager._ReservesLineup[i];
            var display = reserveDisplayList[i];
            display.gameObject.SetActive(true);
            display.displayBanner.sprite = hero.charBanner;
        }
    }
    internal void DisplayMisc()
    {
        miscList.miscArea.text = this.gameObject.scene.name;
        miscList.miscZone.text = SpawnPoint.LastSpawnUsed != null ? SpawnPoint.LastSpawnUsed.Reference.SpawnName : "Unknown";
        miscList.miscCurrency.text = InventoryManager.creditsInBag.ToString();
    }
}
