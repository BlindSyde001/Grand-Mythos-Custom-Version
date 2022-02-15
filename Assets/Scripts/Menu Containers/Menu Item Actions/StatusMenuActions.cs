using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class StatusMenuActions : MonoBehaviour
{
    // VARIABLES
    [SerializeField]
    private GameManager gameManager;
    private InputManager inputManager;
    private MenuInputs menuInputs;

    public TextMeshProUGUI totalExp;
    public TextMeshProUGUI nextLevelExp;

    [SerializeField]
    private StatusContainer statusContainer;

    public List<Button> heroSelections;

    private HeroExtension selectedHero;
    // UPDATES
    private void Start()
    {
        gameManager = GameManager._instance;
        inputManager = InputManager._instance;
        menuInputs = FindObjectOfType<MenuInputs>();
    }

    // METHODS
    internal IEnumerator StatusMenuOpen()
    {
        if (!menuInputs.coroutineRunning)
        {
            yield return new WaitForSeconds(menuInputs.speed);
            inputManager.MenuItems[4].SetActive(true);
            inputManager.MenuItems[4].transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.speed);
            inputManager.MenuItems[4].transform.GetChild(1).DOLocalMove(new Vector3(500, 470, 0), menuInputs.speed);
            inputManager.MenuItems[4].transform.GetChild(2).DOLocalMove(new Vector3(-600, -300, 0), menuInputs.speed);
            inputManager.MenuItems[4].transform.GetChild(3).DOLocalMove(new Vector3(20, -300, 0), menuInputs.speed);
            inputManager.MenuItems[4].transform.GetChild(4).DOLocalMove(new Vector3(640, -300, 0), menuInputs.speed);
            inputManager.MenuItems[4].transform.GetChild(5).DOLocalMove(new Vector3(-600, -45, 0), menuInputs.speed);
            SetExperience(gameManager._PartyLineup[0]);
            SetAttributes(gameManager._PartyLineup[0]);
            SetElemental(gameManager._PartyLineup[0]);
            SetAffliction(gameManager._PartyLineup[0]);
            SetHeroSelection();
        }
    }
    internal IEnumerator StatusMenuClose(bool closeAllOverride)
    {
        if (!menuInputs.coroutineRunning)
        {
            menuInputs.coroutineRunning = true;
            inputManager.MenuItems[4].transform.GetChild(0).DOLocalMove(new Vector3(-1200, 480, 0), menuInputs.speed);
            inputManager.MenuItems[4].transform.GetChild(1).DOLocalMove(new Vector3(500, 610, 0), menuInputs.speed);
            inputManager.MenuItems[4].transform.GetChild(2).DOLocalMove(new Vector3(-600, -800, 0), menuInputs.speed);
            inputManager.MenuItems[4].transform.GetChild(3).DOLocalMove(new Vector3(20, -800, 0), menuInputs.speed);
            inputManager.MenuItems[4].transform.GetChild(4).DOLocalMove(new Vector3(640, -800, 0), menuInputs.speed);
            inputManager.MenuItems[4].transform.GetChild(5).DOLocalMove(new Vector3(-600, -645, 0), menuInputs.speed);
            yield return new WaitForSeconds(menuInputs.speed);
            inputManager.MenuItems[4].SetActive(false);
            menuInputs.coroutineRunning = false;
        }
        if (!closeAllOverride)
        {
            menuInputs.startMenuActions.StartMenuOpen();
            yield return new WaitForSeconds(menuInputs.speed);
            menuInputs.currentMenuOpen = 0;
        }
    }

    internal void SetHeroSelection()
    {
        foreach (Button a in heroSelections)
        {
            a.gameObject.SetActive(false);
            a.onClick.RemoveAllListeners();
        }
        for (int i = 0; i < gameManager._PartyLineup.Count; i++)
        {
            int j = i;
            heroSelections[i].gameObject.SetActive(true);
            heroSelections[i].onClick.AddListener(delegate { SetExperience(gameManager._PartyLineup[j]); });
            heroSelections[i].onClick.AddListener(delegate { SetAttributes(gameManager._PartyLineup[j]); });
            heroSelections[i].onClick.AddListener(delegate { SetElemental(gameManager._PartyLineup[j]); });
            heroSelections[i].onClick.AddListener(delegate { SetAffliction(gameManager._PartyLineup[j]); });
        }
    }

    public void SetExperience(HeroExtension hero)
    {
        totalExp.text = hero._TotalExperience.ToString();
        nextLevelExp.text = (hero.ExperienceThreshold - hero._TotalExperience).ToString();
    }
    public void SetAttributes(HeroExtension hero)
    {
        statusContainer.HP.text = hero.MaxHP.ToString();
        statusContainer.MP.text = hero.MaxMP.ToString();
        statusContainer.Atk.text = hero.Attack.ToString();
        statusContainer.MAtk.text = hero.MagAttack.ToString();
        statusContainer.Def.text = hero.Defense.ToString();
        statusContainer.MDef.text = hero.MagDefense.ToString();
        statusContainer.Spd.text = hero.Speed.ToString();

    }
    public void SetElemental(HeroExtension hero)
    {
        statusContainer.fireRes.text = hero._AffinityFIRE.ToString();
        statusContainer.iceRes.text = hero._AffinityICE.ToString();
        statusContainer.waterRes.text = hero._AffinityWATER.ToString();
        statusContainer.lightRes.text = hero._AffinityLIGHTNING.ToString();
    }
    public void SetAffliction(HeroExtension hero)
    {
        statusContainer.blindRes.text = hero._ResistBLIND.ToString();
        statusContainer.silRes.text = hero._ResistSILENCE.ToString();
        statusContainer.furRes.text = hero._ResistFUROR.ToString();
        statusContainer.parRes.text = hero._ResistPARALYSIS.ToString();
        statusContainer.physRes.text = hero._ResistPHYSICAL.ToString();
        statusContainer.magRes.text = hero._ResistMAGICAL.ToString();
    }
}
