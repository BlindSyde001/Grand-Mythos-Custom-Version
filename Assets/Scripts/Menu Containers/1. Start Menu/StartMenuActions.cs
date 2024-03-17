using System.Collections;
using UnityEngine;
using DG.Tweening;

public class StartMenuActions : MenuContainer
{
    public UIElementList<PartyContainer> PartyUI = new();
    public UIElementList<ReserveContainer> ReserveUI = new();
    public MiscContainer miscList;

    void LateUpdate()
    {
        miscList.miscTime.text = GameManager.Instance.DurationTotal.ToString(@"hh\:mm\:ss");
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
        PartyUI.Clear();
        foreach (var hero in GameManager.PartyLineup)
        {
            PartyUI.Allocate(out var element);
            element.displayName.text = hero.gameObject.name;
            element.displayBanner.sprite = hero.Banner;
            element.displayLevel.text = hero.Level.ToString();

            element.displayEXPBar.fillAmount = (float)(hero.Experience - hero.PrevExperienceThreshold) /
                                               (hero.ExperienceThreshold - hero.PrevExperienceThreshold);

            element.displayHP.text = $"{hero.CurrentHP} / {hero.EffectiveStats.HP}";
        }

        ReserveUI.Clear();
        foreach (var hero in GameManager.ReservesLineup)
        {
            ReserveUI.Allocate(out var display);
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
