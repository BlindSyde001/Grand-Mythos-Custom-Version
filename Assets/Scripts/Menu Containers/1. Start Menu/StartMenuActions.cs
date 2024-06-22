using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;

public class StartMenuActions : MenuContainer
{
    public UIElementList<PartyContainer> PartyUI = new();
    public UIElementList<ReserveContainer> ReserveUI = new();
    [Required] public MiscContainer miscList;
    [Required] public TMP_Text HunterRank;
    [Required] public Image HunterExpBar;

    ((List<HeroExtension> collection, int index) sourceA, (List<HeroExtension> collection, int index) sourceB) _lineupChange;

    void LateUpdate()
    {
        miscList.miscTime.text = GameManager.Instance.DurationTotal.ToString(@"hh\:mm\:ss");
        var rank = SingletonManager.Instance.HunterRank.Amount;
        var exp = SingletonManager.Instance.HunterExperience.Amount;
        var nextExp = CharacterTemplate.GetAmountOfXPForLevel((uint)rank + 1);
        var prevExp = CharacterTemplate.GetAmountOfXPForLevel((uint)rank);
        if (int.TryParse(HunterRank.text, out var textRank) == false || textRank != rank)
            HunterRank.text = rank.ToString();
        HunterExpBar.fillAmount = (exp - prevExp) / ((float)(nextExp - prevExp));
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
        _lineupChange = default;
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
        for (int i = 0; i < GameManager.PartyLineup.Count; i++)
        {
            var hero = GameManager.PartyLineup[i];
            PartyUI.Allocate(out var element);
            element.displayName.text = hero.gameObject.name;
            element.displayBanner.sprite = hero.Banner;
            element.displayLevel.text = hero.Level.ToString();

            element.displayEXPBar.fillAmount = (float)(hero.Experience - hero.PrevExperienceThreshold) /
                                               (hero.ExperienceThreshold - hero.PrevExperienceThreshold);

            element.displayHP.text = $"{hero.CurrentHP} / {hero.EffectiveStats.HP}";
            int j = i;
            element.ChangeOrderButton.onClick.RemoveAllListeners();
            element.ChangeOrderButton.onClick.AddListener(() => ChangePartyLineup(j));
        }

        ReserveUI.Clear();
        for (int i = 0; i < GameManager.ReservesLineup.Count; i++)
        {
            var hero = GameManager.ReservesLineup[i];
            ReserveUI.Allocate(out var display);
            display.displayBanner.sprite = hero.Banner;
            int j = i;
            display.ChangeOrderButton.onClick.AddListener(() => ChangePartyLineupFromReserve(j));
        }
    }
    internal void DisplayMisc()
    {
        miscList.miscArea.text = this.gameObject.scene.name;
        miscList.miscZone.text = SpawnPoint.LastSpawnUsed != null ? SpawnPoint.LastSpawnUsed.Reference.SpawnName : "Unknown";
        miscList.miscCurrency.text = InventoryManager.Credits.ToString();
    }

    void ChangePartyLineup(int selectedToChange)
    {
        ChangePartyLineup((GameManager.PartyLineup, selectedToChange));
    }

    void ChangePartyLineupFromReserve(int selectedToChange)
    {
        ChangePartyLineup((GameManager.ReservesLineup, selectedToChange));
    }

    void ChangePartyLineup((List<HeroExtension> partyLineup, int selectedToChange) data)
    {
        if (_lineupChange.sourceA.collection == null)
        {
            _lineupChange.sourceA = data;
        }
        else
        {
            _lineupChange.sourceB = data;


            var (sourceA, sourceB) = _lineupChange;
            _lineupChange = default;

            var elementA = sourceA.collection[sourceA.index];
            var elementB = sourceB.collection[sourceB.index];

            sourceA.collection[sourceA.index] = elementB;
            sourceB.collection[sourceB.index] = elementA;

            DisplayPartyHeroes();
        }
    }

    public void CancelPartyLineupChanges()
    {
        _lineupChange = default;
    }
}
