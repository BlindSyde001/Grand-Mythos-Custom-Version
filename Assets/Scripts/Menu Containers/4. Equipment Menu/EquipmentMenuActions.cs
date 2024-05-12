using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class EquipmentMenuActions : MenuContainerWithHeroSelection
{
    public EquipStatsContainer EquipStatsContainer;
    public List<EquipLoadoutContainer> EquipLoadoutContainers;

    public UIElementList<EquipNewItemContainer> EquipNewItemContainers;
    public GameObject EquipNewItemList;

    readonly List<Equipment> _currentlyEquippedGear = new();
    Button _listToggle;

    // METHODS
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        foreach (var yields in base.Open(menuInputs))
        {
            yield return yields;
        }
        EquipNewItemList.SetActive(false);
        UpdateCurrentEquippedGear();
        gameObject.SetActive(true);
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(500, 470, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(-580, -320, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(0, -320, 0), menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
    }

    public override IEnumerable Close(MenuInputs menuInputs)
    {
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(500, 610, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(-1400, -320, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(1200, -320, 0), menuInputs.Speed);
        gameObject.transform.GetChild(3).gameObject.SetActive(false);
        yield return new WaitForSeconds(menuInputs.Speed);
        gameObject.SetActive(false);
    }

    protected override void OnSelectedHeroChanged()
    {
        EquipStatsContainer.baseHPText.text = SelectedHero.BaseStats.HP.ToString();
        EquipStatsContainer.baseMPText.text = SelectedHero.BaseStats.MP.ToString();
        EquipStatsContainer.baseAttackText.text = SelectedHero.BaseStats.Attack.ToString();
        EquipStatsContainer.baseMagAttackText.text = SelectedHero.BaseStats.MagAttack.ToString();
        EquipStatsContainer.baseDefenseText.text = SelectedHero.BaseStats.Defense.ToString();
        EquipStatsContainer.baseMagDefenseText.text = SelectedHero.BaseStats.MagDefense.ToString();
        EquipStatsContainer.baseSpeedText.text = SelectedHero.BaseStats.Speed.ToString();

        EquipStatsContainer.EquipHPText.text = SelectedHero.EquipHP.ToString();
        EquipStatsContainer.EquipMPText.text = SelectedHero.EquipMP.ToString();
        EquipStatsContainer.EquipAttackText.text = SelectedHero.EquipAttack.ToString();
        EquipStatsContainer.EquipMagAttackText.text = SelectedHero.EquipMagAttack.ToString();
        EquipStatsContainer.EquipDefenseText.text = SelectedHero.EquipDefense.ToString();
        EquipStatsContainer.EquipMagDefenseText.text = SelectedHero.EquipMagDefense.ToString();
        EquipStatsContainer.EquipSpeedText.text = SelectedHero.EquipSpeed.ToString();

        EquipStatsContainer.TotalHPText.text = SelectedHero.EffectiveStats.HP.ToString();
        EquipStatsContainer.TotalMPText.text = SelectedHero.EffectiveStats.MP.ToString();
        EquipStatsContainer.TotalAttackText.text = SelectedHero.EffectiveStats.Attack.ToString();
        EquipStatsContainer.TotalMagAttackText.text = SelectedHero.EffectiveStats.MagAttack.ToString();
        EquipStatsContainer.TotalDefenseText.text = SelectedHero.EffectiveStats.Defense.ToString();
        EquipStatsContainer.TotalMagDefenseText.text = SelectedHero.EffectiveStats.MagDefense.ToString();
        EquipStatsContainer.TotalSpeedText.text = SelectedHero.EffectiveStats.Speed.ToString();

        for (int i = 0; i <= (int)ItemSlot.Max; i++)
        {
            EquipLoadoutContainers[i].ThisButton.onClick.RemoveAllListeners();

            var slot = (ItemSlot)i;
            var item = GetItemFromSlot(slot, SelectedHero);
            var container = EquipLoadoutContainers[i];
            container.EquippedName.text = item == null ? "None" : item.name;
            container.thisEquipment = item;
            container.ThisButton.onClick.AddListener(delegate { EquippableItemOpen(slot, container.ThisButton); });
        }
    }

    static Equipment GetItemFromSlot(ItemSlot slot, HeroExtension hero) => slot switch
    {
        ItemSlot.Weapon => hero._Weapon,
        ItemSlot.Armor => hero._Armour,
        ItemSlot.AccessoryOne => hero._AccessoryOne,
        ItemSlot.AccessoryTwo => hero._AccessoryTwo,
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
    };

    public void EquippableItemOpen(ItemSlot equipSlot, Button buttonPressed)
    {
        // Open List
        // Add These Item Types onto the List as Buttons, and when pressed, swap them out for Hero's Loadout
        if(EquipNewItemList.activeSelf && buttonPressed == _listToggle)
        {
            EquipNewItemList.SetActive(false);
            return;
        }
        EquipNewItemList.SetActive(true);
        EquipNewItemContainers.Clear();

        bool setSelection = false;
        switch (equipSlot)
        {
            case ItemSlot.Weapon:
                foreach(var (equipment, _) in InventoryManager.Enumerate<Weapon>())
                    if (equipment.weaponType == SelectedHero.myWeaponType)
                        ButtonSetup(this, equipment, ItemSlot.Weapon, ref setSelection);
                break;

            case ItemSlot.Armor:
                foreach(var (equipment, _) in InventoryManager.Enumerate<Armour>())
                    if (equipment.armourType == SelectedHero.myArmourType)
                        ButtonSetup(this, equipment, ItemSlot.Armor, ref setSelection);
                ButtonSetup(this, null, equipSlot, ref setSelection);
                break;

            case ItemSlot.AccessoryTwo:
            case ItemSlot.AccessoryOne:
                foreach(var (equipment, _) in InventoryManager.Enumerate<Accessory>())
                    ButtonSetup(this, equipment, equipSlot, ref setSelection);
                ButtonSetup(this, null, equipSlot, ref setSelection);
                break;

            default:
                throw new NotImplementedException(equipSlot.ToString());
        }

        _listToggle = buttonPressed;

        static void ButtonSetup(EquipmentMenuActions @this, [CanBeNull] Equipment equipment, ItemSlot slot, ref bool setSelection)
        {
            @this.EquipNewItemContainers.Allocate(out var container);
            container.ThisButton.interactable = equipment == null || @this.CheckOnEquippedGear(equipment);
            container.EquipName.text = equipment == null ? "None" : equipment.name;
            container.ThisEquipment = equipment;
            container.ThisButton.onClick.RemoveAllListeners();
            container.ThisButton.onClick.AddListener(delegate { @this.EquipNewItem(equipment, slot); });
            if (setSelection == false && container.ThisButton.interactable)
            {
                setSelection = true;
                container.ThisButton.Select();
            }
        }
    }

    public void EquipNewItem(Equipment newEquip, ItemSlot slotHint)
    {
        switch(newEquip)
        {
            case Weapon w:
                SelectedHero._Weapon = w;
                break;

            case Armour a:
                SelectedHero._Armour = a;
                break;

            case Accessory acc:
                if (ItemSlot.AccessoryOne == slotHint)
                    SelectedHero._AccessoryOne = acc;
                else
                    SelectedHero._AccessoryTwo = acc;
                break;

            case null:
                switch (slotHint)
                {
                    case ItemSlot.Armor:
                        SelectedHero._Armour = null;
                        break;
                    case ItemSlot.Weapon:
                        // mandatory right now, other systems do not expect those to be null
                        break;
                    case ItemSlot.AccessoryOne:
                        SelectedHero._AccessoryOne = null;
                        break;
                    case ItemSlot.AccessoryTwo:
                        SelectedHero._AccessoryTwo = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(slotHint), slotHint, null);
                }

                break;

            default:
                throw new NotImplementedException(slotHint.ToString());
        }
        UpdateCurrentEquippedGear();
        SelectedHero.EquipStats();
        OnSelectedHeroChanged();
        CloseSwapEquipmentWindow();
    }

    public void CloseSwapEquipmentWindow()
    {
        EquipNewItemList.SetActive(false);
    }

    void UpdateCurrentEquippedGear()
    {
        _currentlyEquippedGear.Clear();
        foreach (var hero in GameManager.PartyLineup)
        {
            // Add Currently Equipped Gear
            _currentlyEquippedGear.Add(hero._Weapon);
            if (hero._Armour != null)
                _currentlyEquippedGear.Add(hero._Armour);
            if (hero._AccessoryOne != null)
                _currentlyEquippedGear.Add(hero._AccessoryOne);
            if (hero._AccessoryTwo != null)
                _currentlyEquippedGear.Add(hero._AccessoryTwo);
        }
    }

    bool CheckOnEquippedGear(Equipment equipment)
    {
        int amountEquipped = 0;

        uint amountInInventory;
        InventoryManager.FindItem(equipment, out amountInInventory);

        foreach (Equipment comparison in _currentlyEquippedGear)
        {
            if (comparison == equipment)
                amountEquipped++;
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
