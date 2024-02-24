using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class StatusMenuActions : MenuContainer
{
    public TextMeshProUGUI totalExp;
    public TextMeshProUGUI nextLevelExp;

    [SerializeField] StatusContainer statusContainer;

    public List<Button> heroSelections;
    HeroExtension selectedHero;

    // METHODS
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        if (!menuInputs.coroutineRunning)
        {
            yield return new WaitForSeconds(menuInputs.speed);
            gameObject.SetActive(true);
            gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.speed);
            gameObject.transform.GetChild(1).DOLocalMove(new Vector3(500, 470, 0), menuInputs.speed);
            gameObject.transform.GetChild(2).DOLocalMove(new Vector3(-600, -300, 0), menuInputs.speed);
            gameObject.transform.GetChild(3).DOLocalMove(new Vector3(20, -300, 0), menuInputs.speed);
            gameObject.transform.GetChild(4).DOLocalMove(new Vector3(640, -300, 0), menuInputs.speed);
            gameObject.transform.GetChild(5).DOLocalMove(new Vector3(-600, -45, 0), menuInputs.speed);
            SetExperience(GameManager.PartyLineup[0]);
            SetAttributes(GameManager.PartyLineup[0]);
            SetElemental(GameManager.PartyLineup[0]);
            SetAffliction(GameManager.PartyLineup[0]);
            SetHeroSelection();
        }
    }
    public override IEnumerable Close(MenuInputs menuInputs)
    {
        if (!menuInputs.coroutineRunning)
        {
            menuInputs.coroutineRunning = true;
            gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-1200, 480, 0), menuInputs.speed);
            gameObject.transform.GetChild(1).DOLocalMove(new Vector3(500, 610, 0), menuInputs.speed);
            gameObject.transform.GetChild(2).DOLocalMove(new Vector3(-600, -800, 0), menuInputs.speed);
            gameObject.transform.GetChild(3).DOLocalMove(new Vector3(20, -800, 0), menuInputs.speed);
            gameObject.transform.GetChild(4).DOLocalMove(new Vector3(640, -800, 0), menuInputs.speed);
            gameObject.transform.GetChild(5).DOLocalMove(new Vector3(-600, -645, 0), menuInputs.speed);
            yield return new WaitForSeconds(menuInputs.speed);
            gameObject.SetActive(false);
            menuInputs.coroutineRunning = false;
        }
    }

    internal void SetHeroSelection()
    {
        foreach (Button a in heroSelections)
        {
            a.gameObject.SetActive(false);
            a.onClick.RemoveAllListeners();
        }
        for (int i = 0; i < GameManager.PartyLineup.Count; i++)
        {
            int j = i;
            heroSelections[i].gameObject.SetActive(true);
            heroSelections[i].GetComponent<Image>().sprite = GameManager.PartyLineup[j].Portrait;
            heroSelections[i].onClick.AddListener(delegate { SetExperience(GameManager.PartyLineup[j]); });
            heroSelections[i].onClick.AddListener(delegate { SetAttributes(GameManager.PartyLineup[j]); });
            heroSelections[i].onClick.AddListener(delegate { SetElemental(GameManager.PartyLineup[j]); });
            heroSelections[i].onClick.AddListener(delegate { SetAffliction(GameManager.PartyLineup[j]); });
        }
    }

    public void SetExperience(HeroExtension hero)
    {
        totalExp.text = hero.Experience.ToString();
        nextLevelExp.text = hero.ExperienceToNextLevel.ToString();
    }
    public void SetAttributes(HeroExtension hero)
    {
        statusContainer.HP.text = hero.EffectiveStats.HP.ToString();
        statusContainer.MP.text = hero.EffectiveStats.MP.ToString();
        statusContainer.Atk.text = hero.EffectiveStats.Attack.ToString();
        statusContainer.MAtk.text = hero.EffectiveStats.MagAttack.ToString();
        statusContainer.Def.text = hero.EffectiveStats.Defense.ToString();
        statusContainer.MDef.text = hero.EffectiveStats.MagDefense.ToString();
        statusContainer.Spd.text = hero.EffectiveStats.Speed.ToString();

    }
    public void SetElemental(HeroExtension hero)
    {
        statusContainer.fireRes.text = hero.AffinityFIRE.ToString();
        statusContainer.iceRes.text = hero.AffinityICE.ToString();
        statusContainer.waterRes.text = hero.AffinityWATER.ToString();
        statusContainer.lightRes.text = hero.AffinityLIGHTNING.ToString();
    }
    public void SetAffliction(HeroExtension hero)
    {
        statusContainer.blindRes.text = hero.ResistBLIND.ToString();
        statusContainer.silRes.text = hero.ResistSILENCE.ToString();
        statusContainer.furRes.text = hero.ResistFUROR.ToString();
        statusContainer.parRes.text = hero.ResistPARALYSIS.ToString();
        statusContainer.physRes.text = hero.ResistPHYSICAL.ToString();
        statusContainer.magRes.text = hero.ResistMAGICAL.ToString();
    }
}
