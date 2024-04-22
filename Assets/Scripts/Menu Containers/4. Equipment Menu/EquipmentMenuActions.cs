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

public class EquipmentMenuActions : MenuContainer
{
    public EquipStatsContainer EquipStatsContainer;
    public List<EquipLoadoutContainer> EquipLoadoutContainers;

    public UIElementList<EquipNewItemContainer> EquipNewItemContainers;
    public GameObject EquipNewItemList;

    [FormerlySerializedAs("HeroSelections")] public UIElementList<SelectedHeroView> HeroSelectionUI;
    [Required] public InputActionReference SwitchHero;

    readonly List<Equipment> _currentlyEquippedGear = new();
    Button _listToggle;
    HeroExtension _selectedHero;

    // METHODS
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        SetupHeroSelectionUI();
        UpdateSelection(GameManager.PartyLineup[0]);
        UpdateCurrentEquippedGear();
        gameObject.SetActive(true);
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(500, 470, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(-580, -320, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(0, -320, 0), menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
        SwitchHero.action.performed += Switch;
    }

    public override IEnumerable Close(MenuInputs menuInputs)
    {
        SwitchHero.action.performed -= Switch;
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(500, 610, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(-1400, -320, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(1200, -320, 0), menuInputs.Speed);
        gameObject.transform.GetChild(3).gameObject.SetActive(false);
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

    void SetupHeroSelectionUI()
    {
        HeroSelectionUI.Clear();
        foreach (var hero in GameManager.PartyLineup)
        {
            HeroSelectionUI.Allocate(out var element);
            element.GetComponent<Image>().sprite = hero.Portrait;
            element.Button.onClick.AddListener(delegate {UpdateSelection(hero); });
        }
        EquipNewItemList.SetActive(false);
    }

    public void UpdateSelection(HeroExtension hero)
    {
        _selectedHero = hero;

        EquipStatsContainer.baseHPText.text = hero.BaseStats.HP.ToString();
        EquipStatsContainer.baseMPText.text = hero.BaseStats.MP.ToString();
        EquipStatsContainer.baseAttackText.text = hero.BaseStats.Attack.ToString();
        EquipStatsContainer.baseMagAttackText.text = hero.BaseStats.MagAttack.ToString();
        EquipStatsContainer.baseDefenseText.text = hero.BaseStats.Defense.ToString();
        EquipStatsContainer.baseMagDefenseText.text = hero.BaseStats.MagDefense.ToString();
        EquipStatsContainer.baseSpeedText.text = hero.BaseStats.Speed.ToString();

        EquipStatsContainer.EquipHPText.text = hero.EquipHP.ToString();
        EquipStatsContainer.EquipMPText.text = hero.EquipMP.ToString();
        EquipStatsContainer.EquipAttackText.text = hero.EquipAttack.ToString();
        EquipStatsContainer.EquipMagAttackText.text = hero.EquipMagAttack.ToString();
        EquipStatsContainer.EquipDefenseText.text = hero.EquipDefense.ToString();
        EquipStatsContainer.EquipMagDefenseText.text = hero.EquipMagDefense.ToString();
        EquipStatsContainer.EquipSpeedText.text = hero.EquipSpeed.ToString();

        EquipStatsContainer.TotalHPText.text = hero.EffectiveStats.HP.ToString();
        EquipStatsContainer.TotalMPText.text = hero.EffectiveStats.MP.ToString();
        EquipStatsContainer.TotalAttackText.text = hero.EffectiveStats.Attack.ToString();
        EquipStatsContainer.TotalMagAttackText.text = hero.EffectiveStats.MagAttack.ToString();
        EquipStatsContainer.TotalDefenseText.text = hero.EffectiveStats.Defense.ToString();
        EquipStatsContainer.TotalMagDefenseText.text = hero.EffectiveStats.MagDefense.ToString();
        EquipStatsContainer.TotalSpeedText.text = hero.EffectiveStats.Speed.ToString();

        for (int i = 0; i <= (int)ItemSlot.Max; i++)
        {
            EquipLoadoutContainers[i].ThisButton.onClick.RemoveAllListeners();

            var slot = (ItemSlot)i;
            var item = GetItemFromSlot(slot, hero);
            var container = EquipLoadoutContainers[i];
            container.EquippedName.text = item == null ? "None" : item.name;
            container.thisEquipment = item;
            container.ThisButton.onClick.AddListener(delegate { EquippableItemOpen(slot, container.ThisButton); });
        }

        HighlightSelectedHero(HeroSelectionUI, _selectedHero);
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
                    if (equipment.weaponType == _selectedHero.myWeaponType)
                        ButtonSetup(this, equipment, ItemSlot.Weapon, ref setSelection);
                break;

            case ItemSlot.Armor:
                foreach(var (equipment, _) in InventoryManager.Enumerate<Armour>())
                    if (equipment.armourType == _selectedHero.myArmourType)
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
                _selectedHero._Weapon = w;
                break;

            case Armour a:
                _selectedHero._Armour = a;
                break;

            case Accessory acc:
                if (ItemSlot.AccessoryOne == slotHint)
                    _selectedHero._AccessoryOne = acc;
                else
                    _selectedHero._AccessoryTwo = acc;
                break;

            case null:
                switch (slotHint)
                {
                    case ItemSlot.Armor:
                        _selectedHero._Armour = null;
                        break;
                    case ItemSlot.Weapon:
                        // mandatory right now, other systems do not expect those to be null
                        break;
                    case ItemSlot.AccessoryOne:
                        _selectedHero._AccessoryOne = null;
                        break;
                    case ItemSlot.AccessoryTwo:
                        _selectedHero._AccessoryTwo = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(slotHint), slotHint, null);
                }

                break;

            default:
                throw new NotImplementedException(slotHint.ToString());
        }
        UpdateCurrentEquippedGear();
        _selectedHero.EquipStats();
        UpdateSelection(_selectedHero);
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
