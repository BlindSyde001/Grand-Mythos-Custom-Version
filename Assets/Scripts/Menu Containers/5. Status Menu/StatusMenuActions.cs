using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class StatusMenuActions : MenuContainer
{
    [FormerlySerializedAs("totalExp")] public TextMeshProUGUI TotalExp;
    [FormerlySerializedAs("nextLevelExp")] public TextMeshProUGUI NextLevelExp;

    [FormerlySerializedAs("statusContainer"),SerializeField] StatusContainer StatusContainer;

    [FormerlySerializedAs("HeroSelections")] public UIElementList<Button> HeroSelectionUI;
    [Required] public InputActionReference SwitchHero;
    HeroExtension _selectedHero;

    // METHODS
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        SetupHeroSelectionUI();
        UpdateSelection(GameManager.PartyLineup[0]);

        gameObject.SetActive(true);
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(500, 470, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(-600, -300, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(20, -300, 0), menuInputs.Speed);
        gameObject.transform.GetChild(3).DOLocalMove(new Vector3(640, -300, 0), menuInputs.Speed);
        gameObject.transform.GetChild(4).DOLocalMove(new Vector3(-600, -45, 0), menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
        SwitchHero.action.performed += Switch;
    }

    public override IEnumerable Close(MenuInputs menuInputs)
    {
        SwitchHero.action.performed -= Switch;
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(500, 610, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(-600, -800, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(20, -800, 0), menuInputs.Speed);
        gameObject.transform.GetChild(3).DOLocalMove(new Vector3(640, -800, 0), menuInputs.Speed);
        gameObject.transform.GetChild(4).DOLocalMove(new Vector3(-600, -645, 0), menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
        gameObject.SetActive(false);
    }

    void Switch(InputAction.CallbackContext input)
    {
        int indexOf = GameManager.PartyLineup.IndexOf(_selectedHero);
        indexOf += input.ReadValue<float>() >= 0f ? 1 : -1;
        indexOf = indexOf < 0 ? GameManager.PartyLineup.Count + indexOf : indexOf % GameManager.PartyLineup.Count;

        UpdateSelection(GameManager.PartyLineup[indexOf]);
    }

    internal void SetupHeroSelectionUI()
    {
        HeroSelectionUI.Clear();
        foreach (var hero in GameManager.PartyLineup)
        {
            HeroSelectionUI.Allocate(out var element);
            element.GetComponent<Image>().sprite = hero.Portrait;
            element.onClick.AddListener(delegate { UpdateSelection(hero); });
        }
    }

    public void UpdateSelection(HeroExtension hero)
    {
        _selectedHero = hero;
        TotalExp.text = hero.Experience.ToString();
        NextLevelExp.text = hero.ExperienceToNextLevel.ToString();

        StatusContainer.HP.text = hero.EffectiveStats.HP.ToString();
        StatusContainer.MP.text = hero.EffectiveStats.MP.ToString();
        StatusContainer.Atk.text = hero.EffectiveStats.Attack.ToString();
        StatusContainer.MAtk.text = hero.EffectiveStats.MagAttack.ToString();
        StatusContainer.Def.text = hero.EffectiveStats.Defense.ToString();
        StatusContainer.MDef.text = hero.EffectiveStats.MagDefense.ToString();
        StatusContainer.Spd.text = hero.EffectiveStats.Speed.ToString();

        StatusContainer.fireRes.text = hero.AffinityFIRE.ToString();
        StatusContainer.iceRes.text = hero.AffinityICE.ToString();
        StatusContainer.waterRes.text = hero.AffinityWATER.ToString();
        StatusContainer.lightRes.text = hero.AffinityLIGHTNING.ToString();

        StatusContainer.blindRes.text = hero.ResistBLIND.ToString();
        StatusContainer.silRes.text = hero.ResistSILENCE.ToString();
        StatusContainer.furRes.text = hero.ResistFUROR.ToString();
        StatusContainer.parRes.text = hero.ResistPARALYSIS.ToString();
        StatusContainer.physRes.text = hero.ResistPHYSICAL.ToString();
        StatusContainer.magRes.text = hero.ResistMAGICAL.ToString();

        HighlightSelectedHero(HeroSelectionUI, _selectedHero);
    }
}
