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
    private InventoryManager inventoryManager;
    private MenuInputs menuInputs;

    [SerializeField]
    private EquipStatsContainer equipStatsContainer;
    [SerializeField]
    private List<EquipLoadoutContainer> equipLoadoutContainers;

    [SerializeField]
    private List<EquipNewItemContainer> equipNewItemContainers;
    public GameObject EquipNewItemList;
    private Button listToggle;

    public List<Button> heroSelections;

    private HeroExtension selectedHero;
    // UPDATES
    private void Start()
    {
        gameManager = GameManager._instance;
        inputManager = InputManager._instance;
        inventoryManager = InventoryManager._instance;
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
            inputManager.MenuItems[3].transform.GetChild(3).DOLocalMove(new Vector3(0, -320, 0), menuInputs.speed);
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
            inputManager.MenuItems[3].transform.GetChild(4).gameObject.SetActive(false);
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
        EquipNewItemList.SetActive(false);
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
        for(int i = 0; i < 4; i++)
        {
            int j = i;
            equipLoadoutContainers[i].ThisButton.onClick.RemoveAllListeners();
            switch(i)
            {
                case 0:
                    equipLoadoutContainers[i].EquippedName.text = hero._Weapon._ItemName;
                    equipLoadoutContainers[i].thisEquipment = hero._Weapon;
                    equipLoadoutContainers[i].ThisButton.onClick.AddListener(delegate { EquippableItemOpen(0, equipLoadoutContainers[j].ThisButton); });
                    break;

                case 1:
                    equipLoadoutContainers[i].EquippedName.text = hero._Armour._ItemName;
                    equipLoadoutContainers[i].thisEquipment = hero._Armour;
                    equipLoadoutContainers[i].ThisButton.onClick.AddListener(delegate { EquippableItemOpen(1, equipLoadoutContainers[j].ThisButton); });
                    break;

                case 2:
                    if (hero._AccessoryOne != null)
                    {
                        equipLoadoutContainers[i].EquippedName.text = hero._AccessoryOne._ItemName;
                        equipLoadoutContainers[i].thisEquipment = hero._AccessoryOne;
                    }
                    else
                    {
                        equipLoadoutContainers[i].EquippedName.text = "None";
                    }
                    equipLoadoutContainers[i].ThisButton.onClick.AddListener(delegate { EquippableItemOpen(2, equipLoadoutContainers[j].ThisButton); });
                    break;

                case 3:
                    if (hero._AccessoryTwo != null)
                    {
                        equipLoadoutContainers[i].EquippedName.text = hero._AccessoryTwo._ItemName;
                        equipLoadoutContainers[i].thisEquipment = hero._AccessoryTwo;
                    }
                    else
                    {
                        equipLoadoutContainers[i].EquippedName.text = "None";
                    }
                    equipLoadoutContainers[i].ThisButton.onClick.AddListener(delegate { EquippableItemOpen(3, equipLoadoutContainers[j].ThisButton); });
                    break;
            }
        }
    }

    public void EquippableItemOpen(int equipSlot, Button buttonPressed)
    {
        // Open List
        // Add These Item Types onto the List as Buttons, and when pressed, swap them out for Hero's Loadout
        if(EquipNewItemList.activeSelf == true && buttonPressed == listToggle)
        {
            EquipNewItemList.SetActive(false);
            return;
        }
        EquipNewItemList.SetActive(true);
        foreach(EquipNewItemContainer a in equipNewItemContainers)
        {
            a.EquipName.text = "";
            a.ThisEquipment = null;
        }    
        switch(equipSlot)
        {
            case 0:
                switch(selectedHero._Weapon.name)
                {
                    #region Guns
                    case string a when a.Contains("Gun"):
                        List<Weapon> guns = new();
                        foreach(ItemCapsule weapon in inventoryManager._WeaponsInBag)
                        {
                            if(weapon.thisItem.name.Contains("Gun"))
                            {
                                Weapon wpToAdd = (Weapon)weapon.thisItem;
                                guns.Add(wpToAdd);
                            }
                        }
                        for(int i = 0; i < guns.Count; i++)
                        {
                            int j = i;
                            equipNewItemContainers[i].EquipName.text = guns[i]._ItemName;
                            equipNewItemContainers[i].ThisEquipment = guns[i];
                            equipNewItemContainers[i].ThisButton.onClick.AddListener(delegate { EquipNewItem(guns[j], 0); });
                        }
                        break;
                    #endregion
                    #region Warhammer
                    case string a when a.Contains("Warhammer"):
                        List<Weapon> warhammers = new();
                        foreach (ItemCapsule weapon in inventoryManager._WeaponsInBag)
                        {
                            if (weapon.thisItem.name.Contains("Warhammer"))
                            {
                                Weapon wpToAdd = (Weapon)weapon.thisItem;
                                warhammers.Add(wpToAdd);
                            }
                        }
                        for (int i = 0; i < warhammers.Count; i++)
                        {
                            int j = i;
                            equipNewItemContainers[i].EquipName.text = warhammers[i]._ItemName;
                            equipNewItemContainers[i].ThisEquipment = warhammers[i];
                            equipNewItemContainers[i].ThisButton.onClick.AddListener(delegate { EquipNewItem(warhammers[j], 0); });
                        }
                        break;
                    #endregion
                    #region Power Glove
                    case string a when a.Contains("Power Glove"):
                        List<Weapon> powergloves = new();
                        foreach (ItemCapsule weapon in inventoryManager._WeaponsInBag)
                        {
                            if (weapon.thisItem.name.Contains("Power Glove"))
                            {
                                Weapon wpToAdd = (Weapon)weapon.thisItem;
                                powergloves.Add(wpToAdd);
                            }
                        }
                        for (int i = 0; i < powergloves.Count; i++)
                        {
                            int j = i;
                            equipNewItemContainers[i].EquipName.text = powergloves[i]._ItemName;
                            equipNewItemContainers[i].ThisEquipment = powergloves[i];
                            equipNewItemContainers[i].ThisButton.onClick.AddListener(delegate { EquipNewItem(powergloves[j], 0); });
                        }
                        break;
                    #endregion
                    #region Grimoire
                    case string a when a.Contains("Grimoire"):
                        List<Weapon> grimoire = new();
                        foreach (ItemCapsule weapon in inventoryManager._WeaponsInBag)
                        {
                            if (weapon.thisItem.name.Contains("Grimoire"))
                            {
                                Weapon wpToAdd = (Weapon)weapon.thisItem;
                                grimoire.Add(wpToAdd);
                            }
                        }
                        for (int i = 0; i < grimoire.Count; i++)
                        {
                            int j = i;
                            equipNewItemContainers[i].EquipName.text = grimoire[i]._ItemName;
                            equipNewItemContainers[i].ThisEquipment = grimoire[i];
                            equipNewItemContainers[i].ThisButton.onClick.AddListener(delegate { EquipNewItem(grimoire[j], 0); });
                        }
                        break;
                        #endregion
                }
                break;

            case 1:
                switch(selectedHero._Armour.name)
                {
                    #region Leather
                    case string a when a.Contains("Leather"):
                        List<Armour> leather = new();
                        foreach (ItemCapsule armour in inventoryManager._ArmourInBag)
                        {
                            if (armour.thisItem.name.Contains("Leather"))
                            {
                                Armour armToAdd = (Armour)armour.thisItem;
                                leather.Add(armToAdd);
                            }
                        }
                        for (int i = 0; i < leather.Count; i++)
                        {
                            int j = i;
                            equipNewItemContainers[i].EquipName.text = leather[i]._ItemName;
                            equipNewItemContainers[i].ThisEquipment = leather[i];
                            equipNewItemContainers[i].ThisButton.onClick.AddListener(delegate { EquipNewItem(leather[j], 1); });
                        }
                        break;
                    #endregion
                    #region Mail
                    case string a when a.Contains("Mail"):
                        List<Armour> mail = new();
                        foreach (ItemCapsule armour in inventoryManager._ArmourInBag)
                        {
                            if (armour.thisItem.name.Contains("Mail"))
                            {
                                Armour armToAdd = (Armour)armour.thisItem;
                                mail.Add(armToAdd);
                            }
                        }
                        for (int i = 0; i < mail.Count; i++)
                        {
                            int j = i;
                            equipNewItemContainers[i].EquipName.text = mail[i]._ItemName;
                            equipNewItemContainers[i].ThisEquipment = mail[i];
                            equipNewItemContainers[i].ThisButton.onClick.AddListener(delegate { EquipNewItem(mail[j], 1); });
                        }
                        break;
                    #endregion
                    #region Chasis
                    case string a when a.Contains("Chasis"):
                        List<Armour> chasis = new();
                        foreach (ItemCapsule armour in inventoryManager._ArmourInBag)
                        {
                            if (armour.thisItem.name.Contains("Chasis"))
                            {
                                Armour armToAdd = (Armour)armour.thisItem;
                                chasis.Add(armToAdd);
                            }
                        }
                        for (int i = 0; i < chasis.Count; i++)
                        {
                            int j = i;
                            equipNewItemContainers[i].EquipName.text = chasis[i]._ItemName;
                            equipNewItemContainers[i].ThisEquipment = chasis[i];
                            equipNewItemContainers[i].ThisButton.onClick.AddListener(delegate { EquipNewItem(chasis[j], 1); });
                        }
                        break;
                    #endregion
                    #region Robe
                    case string a when a.Contains("Robe"):
                        List<Armour> robe = new();
                        foreach (ItemCapsule armour in inventoryManager._ArmourInBag)
                        {
                            if (armour.thisItem.name.Contains("Robe"))
                            {
                                Armour armToAdd = (Armour)armour.thisItem;
                                robe.Add(armToAdd);
                            }
                        }
                        for (int i = 0; i < robe.Count; i++)
                        {
                            int j = i;
                            equipNewItemContainers[i].EquipName.text = robe[i]._ItemName;
                            equipNewItemContainers[i].ThisEquipment = robe[i];
                            equipNewItemContainers[i].ThisButton.onClick.AddListener(delegate { EquipNewItem(robe[j], 1); });
                        }
                        break;
                    #endregion
                }
                break;

            case > 1:
                #region Accessory
                for (int i = 0; i < inventoryManager._AccessoryInBag.Count; i++)
                {
                    int j = i;
                    Accessory accessory = (Accessory)inventoryManager._AccessoryInBag[i].thisItem;
                    equipNewItemContainers[i].EquipName.text = accessory._ItemName;
                    equipNewItemContainers[i].ThisEquipment = accessory;
                    equipNewItemContainers[i].ThisButton.onClick.AddListener(delegate { EquipNewItem(accessory, equipSlot); });
                }
                #endregion
                break;
        }
        listToggle = buttonPressed;
    }
    public void EquipNewItem(Equipment newEquip, int type)
    {
        switch(type)
        {
            case 0:
                selectedHero._Weapon = newEquip as Weapon;
                break;
                
            case 1:
                selectedHero._Armour = newEquip as Armour;
                break;

            case 2:
                selectedHero._AccessoryOne = newEquip as Accessory;
                break;

            case 3:
                selectedHero._AccessoryTwo = newEquip as Accessory;
                break;
        }
        selectedHero.EquipStats();
        SetStats(selectedHero);
        SetLoadout(selectedHero);
        EquipNewItemList.SetActive(false);
    }
}
