using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EquipmentMenuActions : MenuContainer
{
    [SerializeField] EquipStatsContainer equipStatsContainer;
    [SerializeField] List<EquipLoadoutContainer> equipLoadoutContainers;

    [SerializeField] List<EquipNewItemContainer> equipNewItemContainers;
    List<Equipment> CurrentlyEquippedGear = new();
    public GameObject EquipNewItemList;
    Button listToggle;

    public List<Button> heroSelections;
    HeroExtension selectedHero;

    // METHODS
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        gameObject.SetActive(true);
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(500, 470, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(-580, -320, 0), menuInputs.Speed);
        gameObject.transform.GetChild(3).DOLocalMove(new Vector3(0, -320, 0), menuInputs.Speed);
        SetStats(GameManager.PartyLineup[0]);
        SetLoadout(GameManager.PartyLineup[0]);
        UpdateCurrentEquippedGear();
        SetHeroSelection();
        yield return new WaitForSeconds(menuInputs.Speed);
    }
    public override IEnumerable Close(MenuInputs menuInputs)
    {
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-1200, 480, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(500, 610, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(-1400, -320, 0), menuInputs.Speed);
        gameObject.transform.GetChild(3).DOLocalMove(new Vector3(1200, -320, 0), menuInputs.Speed);
        gameObject.transform.GetChild(4).gameObject.SetActive(false);
        yield return new WaitForSeconds(menuInputs.Speed);
        gameObject.SetActive(false);
    }

    internal void SetHeroSelection()
    {
        foreach(Button a in heroSelections)
        {
            a.gameObject.SetActive(false);
            a.onClick.RemoveAllListeners();
        }
        for(int i = 0; i < GameManager.PartyLineup.Count; i++)
        {
            int j = i;
            heroSelections[i].gameObject.SetActive(true);
            heroSelections[i].GetComponent<Image>().sprite = GameManager.PartyLineup[j].Portrait;
            heroSelections[i].onClick.AddListener(delegate {SetStats(GameManager.PartyLineup[j]); });
            heroSelections[i].onClick.AddListener(delegate {SetLoadout(GameManager.PartyLineup[j]); });
        }
        EquipNewItemList.SetActive(false);
    }
    public void SetStats(HeroExtension hero)
    {
        selectedHero = hero;

        equipStatsContainer.baseHPText.text = hero.BaseStats.HP.ToString();
        equipStatsContainer.baseMPText.text = hero.BaseStats.MP.ToString();
        equipStatsContainer.baseAttackText.text = hero.BaseStats.Attack.ToString();
        equipStatsContainer.baseMagAttackText.text = hero.BaseStats.MagAttack.ToString();
        equipStatsContainer.baseDefenseText.text = hero.BaseStats.Defense.ToString();
        equipStatsContainer.baseMagDefenseText.text = hero.BaseStats.MagDefense.ToString();
        equipStatsContainer.baseSpeedText.text = hero.BaseStats.Speed.ToString();

        equipStatsContainer.EquipHPText.text = hero.EquipHP.ToString();
        equipStatsContainer.EquipMPText.text = hero.EquipMP.ToString();
        equipStatsContainer.EquipAttackText.text = hero.EquipAttack.ToString();
        equipStatsContainer.EquipMagAttackText.text = hero.EquipMagAttack.ToString();
        equipStatsContainer.EquipDefenseText.text = hero.EquipDefense.ToString();
        equipStatsContainer.EquipMagDefenseText.text = hero.EquipMagDefense.ToString();
        equipStatsContainer.EquipSpeedText.text = hero.EquipSpeed.ToString();

        equipStatsContainer.TotalHPText.text = hero.EffectiveStats.HP.ToString();
        equipStatsContainer.TotalMPText.text = hero.EffectiveStats.MP.ToString();
        equipStatsContainer.TotalAttackText.text = hero.EffectiveStats.Attack.ToString();
        equipStatsContainer.TotalMagAttackText.text = hero.EffectiveStats.MagAttack.ToString();
        equipStatsContainer.TotalDefenseText.text = hero.EffectiveStats.Defense.ToString();
        equipStatsContainer.TotalMagDefenseText.text = hero.EffectiveStats.MagDefense.ToString();
        equipStatsContainer.TotalSpeedText.text = hero.EffectiveStats.Speed.ToString();
    }

    static Equipment GetItemFromSlot(ItemSlot slot, HeroExtension hero) => slot switch
    {
        ItemSlot.Weapon => hero._Weapon,
        ItemSlot.Armor => hero._Armour,
        ItemSlot.AccessoryOne => hero._AccessoryOne,
        ItemSlot.AccessoryTwo => hero._AccessoryTwo,
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
    };

    public void SetLoadout(HeroExtension hero)
    {
        for (int i = 0; i <= (int)ItemSlot.Max; i++)
        {
            equipLoadoutContainers[i].ThisButton.onClick.RemoveAllListeners();

            var slot = (ItemSlot)i;
            var item = GetItemFromSlot(slot, hero);
            var container = equipLoadoutContainers[i];
            container.EquippedName.text = item == null ? "None" : item.name;
            container.thisEquipment = item;
            container.ThisButton.onClick.AddListener(delegate { EquippableItemOpen(slot, container.ThisButton); });
        }
    }

    public void EquippableItemOpen(ItemSlot equipSlot, Button buttonPressed)
    {
        // Open List
        // Add These Item Types onto the List as Buttons, and when pressed, swap them out for Hero's Loadout
        if(EquipNewItemList.activeSelf && buttonPressed == listToggle)
        {
            EquipNewItemList.SetActive(false);
            return;
        }
        EquipNewItemList.SetActive(true);
        foreach(EquipNewItemContainer a in equipNewItemContainers)
        {
            a.EquipName.text = "";
            a.ThisEquipment = null;
            a.ThisButton.onClick.RemoveAllListeners();
            a.ThisButton.onClick.AddListener(() => { EquipNewItem(null, equipSlot); });
        }

        int i = 0;
        switch (equipSlot)
        {
            case ItemSlot.Weapon:
                foreach(var (equipment, _) in InventoryManager.Enumerate<Weapon>())
                {
                    if (equipment.weaponType == selectedHero.myWeaponType)
                    {
                        equipNewItemContainers[i].ThisButton.interactable = CheckOnEquippedGear(equipment);
                        equipNewItemContainers[i].EquipName.text = equipment.name;
                        equipNewItemContainers[i].ThisEquipment = equipment;
                        equipNewItemContainers[i].ThisButton.onClick.AddListener(delegate { EquipNewItem(equipment, ItemSlot.Weapon); });
                        i++;
                    }
                }
                break;

            case ItemSlot.Armor:
                foreach(var (equipment, _) in InventoryManager.Enumerate<Armour>())
                {
                    if (equipment.armourType == selectedHero.myArmourType)
                    {
                        equipNewItemContainers[i].ThisButton.interactable = CheckOnEquippedGear(equipment);
                        equipNewItemContainers[i].EquipName.text = equipment.name;
                        equipNewItemContainers[i].ThisEquipment = equipment;
                        equipNewItemContainers[i].ThisButton.onClick.AddListener(delegate { EquipNewItem(equipment, ItemSlot.Armor); });
                        i++;
                    }
                }
                break;

            case ItemSlot.AccessoryTwo:
            case ItemSlot.AccessoryOne:
                foreach(var (equipment, _) in InventoryManager.Enumerate<Accessory>())
                {
                    equipNewItemContainers[i].ThisButton.interactable = CheckOnEquippedGear(equipment);
                    equipNewItemContainers[i].EquipName.text = equipment.name;
                    equipNewItemContainers[i].ThisEquipment = equipment;
                    equipNewItemContainers[i].ThisButton.onClick.AddListener(delegate { EquipNewItem(equipment, equipSlot); });
                    i++;
                }
                break;

            default:
                throw new NotImplementedException(equipSlot.ToString());
        }

        listToggle = buttonPressed;
    }

    public void EquipNewItem(Equipment newEquip, ItemSlot slotHint)
    {
        switch(newEquip)
        {
            case Weapon w:
                selectedHero._Weapon = w;
                break;

            case Armour a:
                selectedHero._Armour = a;
                break;

            case Accessory acc:
                if (ItemSlot.AccessoryOne == slotHint)
                    selectedHero._AccessoryOne = acc;
                else
                    selectedHero._AccessoryTwo = acc;
                break;

            case null:
                switch (slotHint)
                {
                    case ItemSlot.Armor:
                    case ItemSlot.Weapon:
                        // mandatory right now, other systems do not expect those to be null
                        break;
                    case ItemSlot.AccessoryOne:
                        selectedHero._AccessoryOne = null;
                        break;
                    case ItemSlot.AccessoryTwo:
                        selectedHero._AccessoryTwo = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(slotHint), slotHint, null);
                }

                break;

            default:
                throw new NotImplementedException(slotHint.ToString());
        }
        UpdateCurrentEquippedGear();
        selectedHero.EquipStats();
        SetStats(selectedHero);
        SetLoadout(selectedHero);
        EquipNewItemList.SetActive(false);
    }

    void UpdateCurrentEquippedGear()
    {
        CurrentlyEquippedGear.Clear();
        foreach (var hero in GameManager.PartyLineup)
        {
            // Add Currently Equipped Gear
            CurrentlyEquippedGear.Add(hero._Weapon);
            CurrentlyEquippedGear.Add(hero._Armour);
            if (hero._AccessoryOne != null)
            {
                CurrentlyEquippedGear.Add(hero._AccessoryOne);
            }
            if (hero._AccessoryTwo != null)
            {
                CurrentlyEquippedGear.Add(hero._AccessoryTwo);
            }
        }
    }

    bool CheckOnEquippedGear(Equipment equipment)
    {
        int amountEquipped = 0;

        uint amountInInventory;
        InventoryManager.FindItem(equipment, out amountInInventory);

        foreach (Equipment comparison in CurrentlyEquippedGear)
        {
            if(comparison == equipment)
            {
                amountEquipped++;
            }
        }
        return amountEquipped < amountInInventory;
    }

    public enum ItemSlot
    {
        Weapon = 0,
        Armor = 1,
        AccessoryOne = 2,
        AccessoryTwo = 3,
        Max = 3,
    }
}
