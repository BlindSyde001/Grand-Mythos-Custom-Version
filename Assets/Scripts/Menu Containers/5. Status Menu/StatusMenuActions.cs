using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.Serialization;

public class StatusMenuActions : MenuContainer
{
    [FormerlySerializedAs("totalExp")] public TextMeshProUGUI TotalExp;
    [FormerlySerializedAs("nextLevelExp")] public TextMeshProUGUI NextLevelExp;

    [FormerlySerializedAs("statusContainer"),SerializeField] StatusContainer StatusContainer;

    public UIElementList<Button> HeroSelections;
    HeroExtension _selectedHero;

    // METHODS
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        SetExperience(GameManager.PartyLineup[0]);
        SetAttributes(GameManager.PartyLineup[0]);
        SetElemental(GameManager.PartyLineup[0]);
        SetAffliction(GameManager.PartyLineup[0]);
        SetHeroSelection();
        gameObject.SetActive(true);
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(500, 470, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(-600, -300, 0), menuInputs.Speed);
        gameObject.transform.GetChild(3).DOLocalMove(new Vector3(20, -300, 0), menuInputs.Speed);
        gameObject.transform.GetChild(4).DOLocalMove(new Vector3(640, -300, 0), menuInputs.Speed);
        gameObject.transform.GetChild(5).DOLocalMove(new Vector3(-600, -45, 0), menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
    }
    public override IEnumerable Close(MenuInputs menuInputs)
    {
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-1200, 480, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(500, 610, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(-600, -800, 0), menuInputs.Speed);
        gameObject.transform.GetChild(3).DOLocalMove(new Vector3(20, -800, 0), menuInputs.Speed);
        gameObject.transform.GetChild(4).DOLocalMove(new Vector3(640, -800, 0), menuInputs.Speed);
        gameObject.transform.GetChild(5).DOLocalMove(new Vector3(-600, -645, 0), menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
        gameObject.SetActive(false);
    }

    internal void SetHeroSelection()
    {
        HeroSelections.Clear();
        foreach (var hero in GameManager.PartyLineup)
        {
            HeroSelections.Allocate(out var element);
            element.GetComponent<Image>().sprite = hero.Portrait;
            element.onClick.AddListener(delegate { SetExperience(hero); });
            element.onClick.AddListener(delegate { SetAttributes(hero); });
            element.onClick.AddListener(delegate { SetElemental(hero); });
            element.onClick.AddListener(delegate { SetAffliction(hero); });
        }
    }

    public void SetExperience(HeroExtension hero)
    {
        TotalExp.text = hero.Experience.ToString();
        NextLevelExp.text = hero.ExperienceToNextLevel.ToString();
    }
    public void SetAttributes(HeroExtension hero)
    {
        StatusContainer.HP.text = hero.EffectiveStats.HP.ToString();
        StatusContainer.MP.text = hero.EffectiveStats.MP.ToString();
        StatusContainer.Atk.text = hero.EffectiveStats.Attack.ToString();
        StatusContainer.MAtk.text = hero.EffectiveStats.MagAttack.ToString();
        StatusContainer.Def.text = hero.EffectiveStats.Defense.ToString();
        StatusContainer.MDef.text = hero.EffectiveStats.MagDefense.ToString();
        StatusContainer.Spd.text = hero.EffectiveStats.Speed.ToString();

    }
    public void SetElemental(HeroExtension hero)
    {
        StatusContainer.fireRes.text = hero.AffinityFIRE.ToString();
        StatusContainer.iceRes.text = hero.AffinityICE.ToString();
        StatusContainer.waterRes.text = hero.AffinityWATER.ToString();
        StatusContainer.lightRes.text = hero.AffinityLIGHTNING.ToString();
    }
    public void SetAffliction(HeroExtension hero)
    {
        StatusContainer.blindRes.text = hero.ResistBLIND.ToString();
        StatusContainer.silRes.text = hero.ResistSILENCE.ToString();
        StatusContainer.furRes.text = hero.ResistFUROR.ToString();
        StatusContainer.parRes.text = hero.ResistPARALYSIS.ToString();
        StatusContainer.physRes.text = hero.ResistPHYSICAL.ToString();
        StatusContainer.magRes.text = hero.ResistMAGICAL.ToString();
    }
}
