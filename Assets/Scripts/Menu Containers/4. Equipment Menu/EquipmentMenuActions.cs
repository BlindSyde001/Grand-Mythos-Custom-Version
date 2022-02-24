using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class EquipmentMenuActions : MonoBehaviour
{
    // VARIABLES
    [SerializeField]
    private GameManager gameManager;
    private InputManager inputManager;
    private MenuInputs menuInputs;

    [SerializeField]
    private EquipStatsContainer equipStatsContainer;
    [SerializeField]
    private EquipLoadoutContainer equipLoadoutContainer;

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
    internal IEnumerator EquipmentMenuOpen()
    {
        if (!menuInputs.coroutineRunning)
        {
            yield return new WaitForSeconds(menuInputs.speed);
            inputManager.MenuItems[3].SetActive(true);
            inputManager.MenuItems[3].transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.speed);
            inputManager.MenuItems[3].transform.GetChild(1).DOLocalMove(new Vector3(500, 470, 0), menuInputs.speed);
            inputManager.MenuItems[3].transform.GetChild(2).DOLocalMove(new Vector3(-580, -320, 0), menuInputs.speed);
            inputManager.MenuItems[3].transform.GetChild(3).DOLocalMove(new Vector3(730, -320, 0), menuInputs.speed);
            SetStats(gameManager._PartyLineup[0]);
            SetLoadout(gameManager._PartyLineup[0]);
            SetHeroSelection();
        }
    }
    internal IEnumerator EquipmentMenuClose(bool closeAllOverride)
    {
        if (!menuInputs.coroutineRunning)
        {
            menuInputs.coroutineRunning = true;
            inputManager.MenuItems[3].transform.GetChild(0).DOLocalMove(new Vector3(-1200, 480, 0), menuInputs.speed);
            inputManager.MenuItems[3].transform.GetChild(1).DOLocalMove(new Vector3(500, 610, 0), menuInputs.speed);
            inputManager.MenuItems[3].transform.GetChild(2).DOLocalMove(new Vector3(-1400, -320, 0), menuInputs.speed);
            inputManager.MenuItems[3].transform.GetChild(3).DOLocalMove(new Vector3(1200, -320, 0), menuInputs.speed);
            yield return new WaitForSeconds(menuInputs.speed);
            inputManager.MenuItems[3].SetActive(false);
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
        foreach(Button a in heroSelections)
        {
            a.gameObject.SetActive(false);
            a.onClick.RemoveAllListeners();
        }
        for(int i = 0; i < gameManager._PartyLineup.Count; i++)
        {
            int j = i;
            heroSelections[i].gameObject.SetActive(true);
            heroSelections[i].GetComponent<Image>().sprite = gameManager._PartyLineup[j].charPortrait;
            heroSelections[i].onClick.AddListener(delegate {SetStats(gameManager._PartyLineup[j]); });
            heroSelections[i].onClick.AddListener(delegate {SetLoadout(gameManager._PartyLineup[j]); });
        }
    }
    public void SetStats(HeroExtension hero)
    {
        selectedHero = hero;

        equipStatsContainer.baseHPText.text = hero.BaseHP.ToString();
        equipStatsContainer.baseMPText.text = hero.BaseMP.ToString();
        equipStatsContainer.baseAttackText.text = hero.BaseAttack.ToString();
        equipStatsContainer.baseMagAttackText.text = hero.BaseMagAttack.ToString();
        equipStatsContainer.baseDefenseText.text = hero.BaseDefense.ToString();
        equipStatsContainer.baseMagDefenseText.text = hero.BaseMagDefense.ToString();
        equipStatsContainer.baseSpeedText.text = hero.BaseSpeed.ToString();

        equipStatsContainer.EquipHPText.text = hero.EquipHP.ToString();
        equipStatsContainer.EquipMPText.text = hero.EquipMP.ToString();
        equipStatsContainer.EquipAttackText.text = hero.EquipAttack.ToString();
        equipStatsContainer.EquipMagAttackText.text = hero.EquipMagAttack.ToString();
        equipStatsContainer.EquipDefenseText.text = hero.EquipDefense.ToString();
        equipStatsContainer.EquipMagDefenseText.text = hero.EquipMagDefense.ToString();
        equipStatsContainer.EquipSpeedText.text = hero.EquipSpeed.ToString();

        equipStatsContainer.TotalHPText.text = hero.MaxHP.ToString();
        equipStatsContainer.TotalMPText.text = hero.MaxMP.ToString();
        equipStatsContainer.TotalAttackText.text = hero.Attack.ToString();
        equipStatsContainer.TotalMagAttackText.text = hero.MagAttack.ToString();
        equipStatsContainer.TotalDefenseText.text = hero.Defense.ToString();
        equipStatsContainer.TotalMagDefenseText.text = hero.MagDefense.ToString();
        equipStatsContainer.TotalSpeedText.text = hero.Speed.ToString();
    }
    public void SetLoadout(HeroExtension hero)
    {
        equipLoadoutContainer.WeaponName.text = hero._Weapon._ItemName;
        equipLoadoutContainer.ArmourName.text = hero._Armour._ItemName;
        if (hero._AccessoryOne != null)
        {
            equipLoadoutContainer.AccessoryOneName.text = hero._AccessoryOne._ItemName;
        } 
        else
        {
            equipLoadoutContainer.AccessoryOneName.text = "None";
        }
        if (hero._AccessoryTwo != null)
        {
            equipLoadoutContainer.AccessoryTwoName.text = hero._AccessoryTwo._ItemName;
        }
        else
        {
            equipLoadoutContainer.AccessoryTwoName.text = "None";
        }
    }
}
