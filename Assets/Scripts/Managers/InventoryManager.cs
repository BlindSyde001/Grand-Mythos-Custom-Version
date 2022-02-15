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

    public void RemoveFromInventory()
    {

    }
}
