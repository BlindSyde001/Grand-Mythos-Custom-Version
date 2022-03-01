using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    // VARIABLES
    public static InventoryManager _instance;

    public List<Consumable> ConsumablesInBag;
    public List<Equipment> EquipmentInBag;
    #region Equipment Category Lists
    [SerializeField]
    internal List<Weapon> _WeaponsInBag;
    [SerializeField]
    internal List<Armour> _ArmourInBag;
    [SerializeField]
    internal List<Accessory> _AccessoryInBag;
    #endregion

    public List<KeyItem> KeyItemsInBag;
    public List<Loot> LootInBag;

    public List<Condition> ConditionsAcquired;

    [SerializeField]
    internal int creditsInBag;

    // UPDATES
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }

    // METHODS
    public void SortInventory(int listToSort)
    {
        // 1. Add items to temp list; 2. Find List within Game manager's Database; 3. Sort By Database Sort;
        List<BaseItem> tempList = new();
        switch(listToSort)
        {
            case 0:
                ConsumablesInBag = ConsumablesInBag.OrderBy(i => i._ItemID).ToList();
                break;
            case 1:
                EquipmentInBag = EquipmentInBag.OrderBy(i => i._ItemType).ThenBy(i => i._ItemID).ToList();
                break;
            case 2:
                KeyItemsInBag = KeyItemsInBag.OrderBy(i => i._ItemID).ToList();
                break;
            case 3:
                LootInBag = LootInBag.OrderBy(i => i._ItemID).ToList();
                break;
        }
    }

    public ItemType SortByItemType(ItemType item1, ItemType item2)
    {
        return (ItemType)item1.CompareTo(item2);
    }

    public void RemoveFromInventory(BaseItem item)
    {
        item._ItemAmount--;
        if(item._ItemAmount <= 0)
        {
            switch(item._ItemType)
            {
                case ItemType.CONSUMABLE:
                    Consumable itemC = item as Consumable;
                    ConsumablesInBag.Remove(itemC);
                    break;

                case ItemType.WEAPON:
                    Weapon itemW = item as Weapon;
                    _WeaponsInBag.Remove(itemW);
                    break;

                case ItemType.ARMOUR:
                    Armour itemA = item as Armour;
                    _ArmourInBag.Remove(itemA);
                    break;

                case ItemType.ACCESSORY:
                    Accessory itemAC = item as Accessory;
                    _AccessoryInBag.Remove(itemAC);
                    break;

                case ItemType.KEYITEM:
                    KeyItem itemK = item as KeyItem;
                    KeyItemsInBag.Remove(itemK);
                    break;

                case ItemType.LOOT:
                    Loot itemL = item as Loot;
                    LootInBag.Remove(itemL);
                    break;
            }
        }
    }
    public void AddToInventory()
    {

    }
}
