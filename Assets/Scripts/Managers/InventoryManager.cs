using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

public class InventoryManager : MonoBehaviour
{
    // VARIABLES
    public static InventoryManager _instance;
    #region Items In Bag
    public List<ItemCapsule> ConsumablesInBag;

    [BoxGroup("Equipment")]
    public List<ItemCapsule> EquipmentInBag;
    [BoxGroup("Equipment")]
    [SerializeField]
    internal List<ItemCapsule> _WeaponsInBag;
    [BoxGroup("Equipment")]
    [SerializeField]
    internal List<ItemCapsule> _ArmourInBag;
    [BoxGroup("Equipment")]
    [SerializeField]
    internal List<ItemCapsule> _AccessoryInBag;

    public List<ItemCapsule> KeyItemsInBag;
    public List<ItemCapsule> LootInBag;
    #endregion

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
                ConsumablesInBag = ConsumablesInBag.OrderBy(i => i.ItemID).ToList();
                break;
            case 1:
                EquipmentInBag = EquipmentInBag.OrderBy(i => i.thisItem._ItemType).ThenBy(i => i.ItemID).ToList();
                break;
            case 2:
                KeyItemsInBag = KeyItemsInBag.OrderBy(i => i.ItemID).ToList();
                break;
            case 3:
                LootInBag = LootInBag.OrderBy(i => i.ItemID).ToList();
                break;
        }
    }

    public ItemType SortByItemType(ItemType item1, ItemType item2)
    {
        return (ItemType)item1.CompareTo(item2);
    }

    public void RemoveFromInventory(ItemCapsule item)
    {
        if (item != null)
        {
            Debug.Log(item.thisItem._ItemName + " has been used");
            item.ItemAmount--;
            if (item.ItemAmount <= 0)
            {
                switch (item.thisItem._ItemType)
                {
                    case ItemType.CONSUMABLE:
                        ConsumablesInBag.Remove(item);
                        break;

                    case ItemType.WEAPON:
                        _WeaponsInBag.Remove(item);
                        break;

                    case ItemType.ARMOUR:
                        _ArmourInBag.Remove(item);
                        break;

                    case ItemType.ACCESSORY:
                        _AccessoryInBag.Remove(item);
                        break;

                    case ItemType.KEYITEM:
                        KeyItemsInBag.Remove(item);
                        break;

                    case ItemType.LOOT:
                        LootInBag.Remove(item);
                        break;
                }
            }
        }
    }
    public void AddToInventory(ItemCapsule item)
    {
        switch(item.thisItem._ItemType)
        {
            case ItemType.CONSUMABLE:
                if(ConsumablesInBag.Find(x => x.thisItem == item.thisItem) != null)
                {
                   ConsumablesInBag.Find(x => x.thisItem == item.thisItem).ItemAmount += item.ItemAmount;
                }
                else
                {
                    ConsumablesInBag.Add(item);
                }
                break;

            case ItemType.WEAPON:
                if (_WeaponsInBag.Find(x => x.thisItem == item.thisItem) != null)
                {
                    EquipmentInBag.Find(x => x.thisItem == item.thisItem).ItemAmount += item.ItemAmount;
                    _WeaponsInBag.Find(x => x.thisItem == item.thisItem).ItemAmount += item.ItemAmount;
                }
                else
                {
                    EquipmentInBag.Add(item);
                    _WeaponsInBag.Add(item);
                }
                break;

            case ItemType.ARMOUR:
                if (_ArmourInBag.Find(x => x.thisItem == item.thisItem) != null)
                {
                    EquipmentInBag.Find(x => x.thisItem == item.thisItem).ItemAmount += item.ItemAmount;
                    _ArmourInBag.Find(x => x.thisItem == item.thisItem).ItemAmount += item.ItemAmount;
                }
                else
                {
                    EquipmentInBag.Add(item);
                    _ArmourInBag.Add(item);
                }
                break;

            case ItemType.ACCESSORY:
                if (_AccessoryInBag.Find(x => x.thisItem == item.thisItem) != null)
                {
                    EquipmentInBag.Find(x => x.thisItem == item.thisItem).ItemAmount += item.ItemAmount;
                    _AccessoryInBag.Find(x => x.thisItem == item.thisItem).ItemAmount += item.ItemAmount;
                }
                else
                {
                    EquipmentInBag.Add(item);
                    _AccessoryInBag.Add(item);
                }
                break;

            case ItemType.KEYITEM:
                if (KeyItemsInBag.Find(x => x.thisItem == item.thisItem) != null)
                {
                    KeyItemsInBag.Find(x => x.thisItem == item.thisItem).ItemAmount += item.ItemAmount;
                }
                else
                {
                    KeyItemsInBag.Add(item);
                }
                break;

            case ItemType.LOOT:
                if (LootInBag.Find(x => x.thisItem == item.thisItem) != null)
                {
                    LootInBag.Find(x => x.thisItem == item.thisItem).ItemAmount += item.ItemAmount;
                }
                else
                {
                    LootInBag.Add(item);
                }
                break;
        }
    }
    public bool CheckInventoryforItem(ItemCapsule item)
    {
        switch (item.thisItem._ItemType)
        {
            case ItemType.CONSUMABLE:
                if (ConsumablesInBag.Find(x => x.thisItem == item.thisItem) != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            case ItemType.WEAPON:
                if (_WeaponsInBag.Find(x => x.thisItem == item.thisItem) != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            case ItemType.ARMOUR:
                if (_ArmourInBag.Find(x => x.thisItem == item.thisItem) != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            case ItemType.ACCESSORY:
                if (_AccessoryInBag.Find(x => x.thisItem == item.thisItem) != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            case ItemType.KEYITEM:
                if (KeyItemsInBag.Find(x => x.thisItem == item.thisItem) != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            case ItemType.LOOT:
                if (LootInBag.Find(x => x.thisItem == item.thisItem) != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
        }
        return false;
    }
}
