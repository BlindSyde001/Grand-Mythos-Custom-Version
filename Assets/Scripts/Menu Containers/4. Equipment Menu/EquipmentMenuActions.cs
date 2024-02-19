using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EquipmentMenuActions : MenuContainer
{
    [SerializeField]
    private EquipStatsContainer equipStatsContainer;
    [SerializeField]
    private List<EquipLoadoutContainer> equipLoadoutContainers;

    [SerializeField]
    private List<EquipNewItemContainer> equipNewItemContainers;
    private List<Equipment> CurrentlyEquippedGear = new();
    public GameObject EquipNewItemList;
    private Button listToggle;

    public List<Button> heroSelections;

    private HeroExtension selectedHero;

    // METHODS
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        if (!menuInputs.coroutineRunning)
        {
            yield return new WaitForSeconds(menuInputs.speed);
            gameObject.SetActive(true);
            gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.speed);
            gameObject.transform.GetChild(1).DOLocalMove(new Vector3(500, 470, 0), menuInputs.speed);
            gameObject.transform.GetChild(2).DOLocalMove(new Vector3(-580, -320, 0), menuInputs.speed);
            gameObject.transform.GetChild(3).DOLocalMove(new Vector3(0, -320, 0), menuInputs.speed);
            SetStats(GameManager._PartyLineup[0]);
            SetLoadout(GameManager._PartyLineup[0]);
            UpdateCurrentEquippedGear();
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
            gameObject.transform.GetChild(2).DOLocalMove(new Vector3(-1400, -320, 0), menuInputs.speed);
            gameObject.transform.GetChild(3).DOLocalMove(new Vector3(1200, -320, 0), menuInputs.speed);
            gameObject.transform.GetChild(4).gameObject.SetActive(false);
            yield return new WaitForSeconds(menuInputs.speed);
            gameObject.SetActive(false);
            menuInputs.coroutineRunning = false;
        }
    }

    internal void SetHeroSelection()
    {
        foreach(Button a in heroSelections)
        {
            a.gameObject.SetActive(false);
            a.onClick.RemoveAllListeners();
        }
        for(int i = 0; i < GameManager._PartyLineup.Count; i++)
        {
            int j = i;
            heroSelections[i].gameObject.SetActive(true);
            heroSelections[i].GetComponent<Image>().sprite = GameManager._PartyLineup[j].charPortrait;
            heroSelections[i].onClick.AddListener(delegate {SetStats(GameManager._PartyLineup[j]); });
            heroSelections[i].onClick.AddListener(delegate {SetLoadout(GameManager._PartyLineup[j]); });
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
                    equipLoadoutContainers[i].EquippedName.text = hero._Weapon.name;
                    equipLoadoutContainers[i].thisEquipment = hero._Weapon;
                    equipLoadoutContainers[i].ThisButton.onClick.AddListener(delegate { EquippableItemOpen(0, equipLoadoutContainers[j].ThisButton); });
                    break;

                case 1:
                    equipLoadoutContainers[i].EquippedName.text = hero._Armour.name;
                    equipLoadoutContainers[i].thisEquipment = hero._Armour;
                    equipLoadoutContainers[i].ThisButton.onClick.AddListener(delegate { EquippableItemOpen(1, equipLoadoutContainers[j].ThisButton); });
                    break;

                case 2:
                    if (hero._AccessoryOne != null)
                    {
                        equipLoadoutContainers[i].EquippedName.text = hero._AccessoryOne.name;
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
                        equipLoadoutContainers[i].EquippedName.text = hero._AccessoryTwo.name;
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
                switch(selectedHero.myWeaponType)
                {
                    #region Guns
                    case Weapon.WeaponType.Gun:
                        List<Weapon> guns = new();
                        foreach(ItemCapsule weapon in InventoryManager._WeaponsInBag)
                        {
                            // Add the Guns
                            if(((Object)weapon.thisItem).name.Contains("Gun"))
                            {
                                Weapon wpToAdd = (Weapon)weapon.thisItem;
                                guns.Add(wpToAdd);
                            }
                        }
                        for(int i = 0; i < guns.Count; i++)
                        {
                            int j = i; 
                            if(CheckOnEquippedGear(guns[i]))
                            {
                                equipNewItemContainers[i].ThisButton.interactable = true;
                            }
                            else
                            {
                                equipNewItemContainers[i].ThisButton.interactable = false;
                            }
                            equipNewItemContainers[i].EquipName.text = guns[i].name;
                            equipNewItemContainers[i].ThisEquipment = guns[i];
                            equipNewItemContainers[i].ThisButton.onClick.AddListener(delegate { EquipNewItem(guns[j], 0); });
                        }
                        break;
                    #endregion
                    #region Warhammer
                    case Weapon.WeaponType.Warhammer:
                        List<Weapon> warhammers = new();
                        foreach (ItemCapsule weapon in InventoryManager._WeaponsInBag)
                        {
                            if (((Object)weapon.thisItem).name.Contains("Warhammer"))
                            {
                                Weapon wpToAdd = (Weapon)weapon.thisItem;
                                warhammers.Add(wpToAdd);
                            }
                        }
                        for (int i = 0; i < warhammers.Count; i++)
                        {
                            if (CheckOnEquippedGear(warhammers[i]))
                            {
                                equipNewItemContainers[i].ThisButton.interactable = true;
                            }
                            else
                            {
                                equipNewItemContainers[i].ThisButton.interactable = false;
                            }
                            int j = i;
                            equipNewItemContainers[i].EquipName.text = warhammers[i].name;
                            equipNewItemContainers[i].ThisEquipment = warhammers[i];
                            equipNewItemContainers[i].ThisButton.onClick.AddListener(delegate { EquipNewItem(warhammers[j], 0); });
                        }
                        break;
                    #endregion
                    #region Power Glove
                    case Weapon.WeaponType.PowerGlove:
                        List<Weapon> powergloves = new();
                        foreach (ItemCapsule weapon in InventoryManager._WeaponsInBag)
                        {
                            if (((Object)weapon.thisItem).name.Contains("Power Glove"))
                            {
                                Weapon wpToAdd = (Weapon)weapon.thisItem;
                                powergloves.Add(wpToAdd);
                            }
                        }
                        for (int i = 0; i < powergloves.Count; i++)
                        {
                            if (CheckOnEquippedGear(powergloves[i]))
                            {
                                equipNewItemContainers[i].ThisButton.interactable = true;
                            }
                            else
                            {
                                equipNewItemContainers[i].ThisButton.interactable = false;
                            }
                            int j = i;
                            equipNewItemContainers[i].EquipName.text = powergloves[i].name;
                            equipNewItemContainers[i].ThisEquipment = powergloves[i];
                            equipNewItemContainers[i].ThisButton.onClick.AddListener(delegate { EquipNewItem(powergloves[j], 0); });
                        }
                        break;
                    #endregion
                    #region Grimoire
                    case Weapon.WeaponType.Grimoire:
                        List<Weapon> grimoire = new();
                        foreach (ItemCapsule weapon in InventoryManager._WeaponsInBag)
                        {
                            if (((Object)weapon.thisItem).name.Contains("Grimoire"))
                            {
                                Weapon wpToAdd = (Weapon)weapon.thisItem;
                                grimoire.Add(wpToAdd);
                            }
                        }
                        for (int i = 0; i < grimoire.Count; i++)
                        {
                            if (CheckOnEquippedGear(grimoire[i]))
                            {
                                equipNewItemContainers[i].ThisButton.interactable = true;
                            }
                            else
                            {
                                equipNewItemContainers[i].ThisButton.interactable = false;
                            }
                            int j = i;
                            equipNewItemContainers[i].EquipName.text = grimoire[i].name;
                            equipNewItemContainers[i].ThisEquipment = grimoire[i];
                            equipNewItemContainers[i].ThisButton.onClick.AddListener(delegate { EquipNewItem(grimoire[j], 0); });
                        }
                        break;
                        #endregion
                }
                break;

            case 1:
                switch(selectedHero.myArmourType)
                {
                    #region Leather
                    case Armour.ArmourType.Leather:
                        List<Armour> leather = new();
                        foreach (ItemCapsule armour in InventoryManager._ArmourInBag)
                        {
                            if (((Object)armour.thisItem).name.Contains("Leather"))
                            {
                                Armour armToAdd = (Armour)armour.thisItem;
                                leather.Add(armToAdd);
                            }
                        }
                        for (int i = 0; i < leather.Count; i++)
                        {
                            if (CheckOnEquippedGear(leather[i]))
                            {
                                equipNewItemContainers[i].ThisButton.interactable = true;
                            }
                            else
                            {
                                equipNewItemContainers[i].ThisButton.interactable = false;
                            }
                            int j = i;
                            equipNewItemContainers[i].EquipName.text = leather[i].name;
                            equipNewItemContainers[i].ThisEquipment = leather[i];
                            equipNewItemContainers[i].ThisButton.onClick.AddListener(delegate { EquipNewItem(leather[j], 1); });
                        }
                        break;
                    #endregion
                    #region Mail
                    case Armour.ArmourType.Mail:
                        List<Armour> mail = new();
                        foreach (ItemCapsule armour in InventoryManager._ArmourInBag)
                        {
                            if (((Object)armour.thisItem).name.Contains("Mail"))
                            {
                                Armour armToAdd = (Armour)armour.thisItem;
                                mail.Add(armToAdd);
                            }
                        }
                        for (int i = 0; i < mail.Count; i++)
                        {
                            if (CheckOnEquippedGear(mail[i]))
                            {
                                equipNewItemContainers[i].ThisButton.interactable = true;
                            }
                            else
                            {
                                equipNewItemContainers[i].ThisButton.interactable = false;
                            }
                            int j = i;
                            equipNewItemContainers[i].EquipName.text = mail[i].name;
                            equipNewItemContainers[i].ThisEquipment = mail[i];
                            equipNewItemContainers[i].ThisButton.onClick.AddListener(delegate { EquipNewItem(mail[j], 1); });
                        }
                        break;
                    #endregion
                    #region Chasis
                    case Armour.ArmourType.Chasis:
                        List<Armour> chasis = new();
                        foreach (ItemCapsule armour in InventoryManager._ArmourInBag)
                        {
                            if (((Object)armour.thisItem).name.Contains("Chasis"))
                            {
                                Armour armToAdd = (Armour)armour.thisItem;
                                chasis.Add(armToAdd);
                            }
                        }
                        for (int i = 0; i < chasis.Count; i++)
                        {
                            if (CheckOnEquippedGear(chasis[i]))
                            {
                                equipNewItemContainers[i].ThisButton.interactable = true;
                            }
                            else
                            {
                                equipNewItemContainers[i].ThisButton.interactable = false;
                            }
                            int j = i;
                            equipNewItemContainers[i].EquipName.text = chasis[i].name;
                            equipNewItemContainers[i].ThisEquipment = chasis[i];
                            equipNewItemContainers[i].ThisButton.onClick.AddListener(delegate { EquipNewItem(chasis[j], 1); });
                        }
                        break;
                    #endregion
                    #region Robe
                    case Armour.ArmourType.Robes:
                        List<Armour> robe = new();
                        foreach (ItemCapsule armour in InventoryManager._ArmourInBag)
                        {
                            if (((Object)armour.thisItem).name.Contains("Robe"))
                            {
                                Armour armToAdd = (Armour)armour.thisItem;
                                robe.Add(armToAdd);
                            }
                        }
                        for (int i = 0; i < robe.Count; i++)
                        {
                            if (CheckOnEquippedGear(robe[i]))
                            {
                                equipNewItemContainers[i].ThisButton.interactable = true;
                            }
                            else
                            {
                                equipNewItemContainers[i].ThisButton.interactable = false;
                            }
                            int j = i;
                            equipNewItemContainers[i].EquipName.text = robe[i].name;
                            equipNewItemContainers[i].ThisEquipment = robe[i];
                            equipNewItemContainers[i].ThisButton.onClick.AddListener(delegate { EquipNewItem(robe[j], 1); });
                        }
                        break;
                    #endregion
                }
                break;

            case > 1:
                #region Accessory
                for (int i = 0; i < InventoryManager._AccessoryInBag.Count; i++)
                {
                    Accessory accessory = (Accessory)InventoryManager._AccessoryInBag[i].thisItem;
                    if (CheckOnEquippedGear(accessory))
                    {
                        equipNewItemContainers[i].ThisButton.interactable = true;
                    }
                    else
                    {
                        equipNewItemContainers[i].ThisButton.interactable = false;
                    }
                    int j = i;
                    equipNewItemContainers[i].EquipName.text = accessory.name;
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
        UpdateCurrentEquippedGear();
        selectedHero.EquipStats();
        SetStats(selectedHero);
        SetLoadout(selectedHero);
        EquipNewItemList.SetActive(false);
    }



    private void UpdateCurrentEquippedGear()
    {
        CurrentlyEquippedGear.Clear();
        for (int i = 0; i < GameManager._PartyLineup.Count; i++)
        {
            // Add Currently Equipped Gear
            CurrentlyEquippedGear.Add(GameManager._PartyLineup[i]._Weapon);
            CurrentlyEquippedGear.Add(GameManager._PartyLineup[i]._Armour);
            if (GameManager._PartyLineup[i]._AccessoryOne != null)
            {
                CurrentlyEquippedGear.Add(GameManager._PartyLineup[i]._AccessoryOne);
            }
            if (GameManager._PartyLineup[i]._AccessoryTwo != null)
            {
                CurrentlyEquippedGear.Add(GameManager._PartyLineup[i]._AccessoryTwo);
            }
        }
    }
    private bool CheckOnEquippedGear(Equipment equipment)
    {
        int amountEquipped = 0;
        int amountInInventory = 0;

        ItemCapsule a = InventoryManager.EquipmentInBag.Find(x => x.thisItem == equipment);
        amountInInventory = a.ItemAmount;

        foreach(Equipment comparison in CurrentlyEquippedGear)
        {
            if(comparison == equipment)
            {
                //Debug.Log("Found One!");
                amountEquipped++;
            }
        }
        if(amountEquipped < amountInInventory)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
