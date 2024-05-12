using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;

public class StatusMenuActions : MenuContainerWithHeroSelection
{
    [Required] public TextMeshProUGUI TotalExp;
    [Required] public TextMeshProUGUI NextLevelExp;

    [SerializeField] StatusContainer StatusContainer;

    // METHODS
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        foreach (var yields in base.Open(menuInputs))
        {
            yield return yields;
        }

        gameObject.SetActive(true);
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(500, 470, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(-600, -300, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(20, -300, 0), menuInputs.Speed);
        gameObject.transform.GetChild(3).DOLocalMove(new Vector3(640, -300, 0), menuInputs.Speed);
        gameObject.transform.GetChild(4).DOLocalMove(new Vector3(-600, -45, 0), menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
    }

    public override IEnumerable Close(MenuInputs menuInputs)
    {
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(500, 610, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(-600, -800, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(20, -800, 0), menuInputs.Speed);
        gameObject.transform.GetChild(3).DOLocalMove(new Vector3(640, -800, 0), menuInputs.Speed);
        gameObject.transform.GetChild(4).DOLocalMove(new Vector3(-600, -645, 0), menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
        gameObject.SetActive(false);
    }

    protected override void OnSelectedHeroChanged()
    {
        TotalExp.text = SelectedHero.Experience.ToString();
        NextLevelExp.text = SelectedHero.ExperienceToNextLevel.ToString();

        StatusContainer.HP.text = SelectedHero.EffectiveStats.HP.ToString();
        StatusContainer.MP.text = SelectedHero.EffectiveStats.MP.ToString();
        StatusContainer.Atk.text = SelectedHero.EffectiveStats.Attack.ToString();
        StatusContainer.MAtk.text = SelectedHero.EffectiveStats.MagAttack.ToString();
        StatusContainer.Def.text = SelectedHero.EffectiveStats.Defense.ToString();
        StatusContainer.MDef.text = SelectedHero.EffectiveStats.MagDefense.ToString();
        StatusContainer.Spd.text = SelectedHero.EffectiveStats.Speed.ToString();

        StatusContainer.fireRes.text = SelectedHero.ResistanceFire.ToString();
        StatusContainer.iceRes.text = SelectedHero.ResistanceIce.ToString();
        StatusContainer.waterRes.text = SelectedHero.ResistanceWater.ToString();
        StatusContainer.lightRes.text = SelectedHero.ResistanceLightning.ToString();

        StatusContainer.blindRes.text = "TBD: Status unimplemented";
        StatusContainer.silRes.text = "TBD: Status unimplemented";
        StatusContainer.furRes.text = "TBD: Status unimplemented";
        StatusContainer.parRes.text = "TBD: Status unimplemented";
        StatusContainer.physRes.text = "TBD: Status unimplemented";
        StatusContainer.magRes.text = "TBD: Status unimplemented";
    }
}
