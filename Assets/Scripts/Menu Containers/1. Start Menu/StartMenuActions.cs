using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StartMenuActions : MenuContainer
{
    [SerializeField] List<PartyContainer> displayList;
    [SerializeField] List<ReserveContainer> reserveDisplayList;
    [SerializeField] MiscContainer miscList;
    [SerializeField] InGameClock inGameClock;

    void LateUpdate()
    {
        miscList.miscTime.text = inGameClock.DurationTotal.ToString(@"hh\:mm\:ss");
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
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-740, 150, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(200, 30, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(200, gameObject.transform.GetChild(2).localPosition.y, 0), menuInputs.Speed);
        gameObject.transform.GetChild(3).DOLocalMove(new Vector3(0, 0, 0), menuInputs.Speed);
        DisplayPartyHeroes();
        DisplayMisc();
        yield return new WaitForSeconds(menuInputs.Speed);
    }
    public override IEnumerable Close(MenuInputs menuInputs)
    {
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-1200, 150, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(1700, 30, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(1700, gameObject.transform.GetChild(2).localPosition.y, 0), menuInputs.Speed);
        gameObject.transform.GetChild(3).DOLocalMove(new Vector3(0, -150, 0), menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
        gameObject.SetActive(false);
    }
    internal void DisplayPartyHeroes()
    {
        foreach (PartyContainer a in displayList)
        {
            a.gameObject.SetActive(false);
        }
        for (int i = 0; i < GameManager.PartyLineup.Count; i++)
        {
            var hero = GameManager.PartyLineup[i];
            displayList[i].gameObject.SetActive(true);
            displayList[i].displayName.text = hero.gameObject.name;
            displayList[i].displayBanner.sprite = hero.Banner;
            displayList[i].displayLevel.text = hero.Level.ToString();

            displayList[i].displayEXPBar.fillAmount = (float)(hero.Experience - hero.PrevExperienceThreshold) /
                                                             (hero.ExperienceThreshold - hero.PrevExperienceThreshold);

            displayList[i].displayHP.text = $"{hero.CurrentHP} / {hero.EffectiveStats.HP}";
        }

        foreach (ReserveContainer a in reserveDisplayList)
        {
            a.gameObject.SetActive(false);
        }
        for (int i = 0; i < GameManager.ReservesLineup.Count; i++)
        {
            var hero = GameManager.ReservesLineup[i];
            var display = reserveDisplayList[i];
            display.gameObject.SetActive(true);
            display.displayBanner.sprite = hero.Banner;
        }
    }
    internal void DisplayMisc()
    {
        miscList.miscArea.text = this.gameObject.scene.name;
        miscList.miscZone.text = SpawnPoint.LastSpawnUsed != null ? SpawnPoint.LastSpawnUsed.Reference.SpawnName : "Unknown";
        miscList.miscCurrency.text = InventoryManager.Credits.ToString();
    }
}
