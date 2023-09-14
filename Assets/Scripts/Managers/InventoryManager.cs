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

    public List<ActionCondition> ConditionsAcquired;

    [SerializeField]
    internal int creditsInBag;

    // UPDATES
    private void OnEnable()
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
                ConsumablesInBag = ConsumablesInBag.OrderBy(i => i.thisItem.guid).ToList();
                break;
            case 1:
                EquipmentInBag = EquipmentInBag.OrderBy(i => i.thisItem.GetType().FullName).ThenBy(i => i.thisItem.guid).ToList();
                break;
            case 2:
                KeyItemsInBag = KeyItemsInBag.OrderBy(i => i.thisItem.guid).ToList();
                break;
            case 3:
                LootInBag = LootInBag.OrderBy(i => i.thisItem.guid).ToList();
                break;
        }
    }

    public void RemoveFromInventory(ItemCapsule item)
    {
        if (item != null)
        {
            Debug.Log(item.thisItem.name + " has been used");
            item.ItemAmount--;
            if (item.ItemAmount <= 0)
            {
                switch (item.thisItem)
                {
                    case Consumable:
                        ConsumablesInBag.Remove(item);
                        break;

                    case Weapon:
                        _WeaponsInBag.Remove(item);
                        break;

                    case Armour:
                        _ArmourInBag.Remove(item);
                        break;

                    case Accessory:
                        _AccessoryInBag.Remove(item);
                        break;

                    case KeyItem:
                        KeyItemsInBag.Remove(item);
                        break;

                    case Loot:
                        LootInBag.Remove(item);
                        break;
                }
            }
        }
    }
    public void AddToInventory(ItemCapsule item)
    {
        switch(item.thisItem)
        {
            case Consumable:
                if(ConsumablesInBag.Find(x => x.thisItem == item.thisItem) != null)
                {
                   ConsumablesInBag.Find(x => x.thisItem == item.thisItem).ItemAmount += item.ItemAmount;
                }
                else
                {
                    ConsumablesInBag.Add(item);
                }
                break;

            case Weapon:
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

            case Armour:
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

            case Accessory:
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

            case KeyItem:
                if (KeyItemsInBag.Find(x => x.thisItem == item.thisItem) != null)
                {
                    KeyItemsInBag.Find(x => x.thisItem == item.thisItem).ItemAmount += item.ItemAmount;
                }
                else
                {
                    KeyItemsInBag.Add(item);
                }
                break;

            case Loot:
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
    public bool FindItem(ItemCapsule item, out uint count)
    {
        count = 0;
        switch (item.thisItem)
        {
            case Consumable:
                if (ConsumablesInBag.Find(x => x.thisItem == item.thisItem) is {} v)
                {
                    count = (uint)v.ItemAmount;
                    return true;
                }
                else
                {
                    return false;
                }

            case Weapon:
                if (_WeaponsInBag.Find(x => x.thisItem == item.thisItem) is {} v2)
                {
                    count = (uint)v2.ItemAmount;
                    return true;
                }
                else
                {
                    return false;
                }

            case Armour:
                if (_ArmourInBag.Find(x => x.thisItem == item.thisItem) is {} v3)
                {
                    count = (uint)v3.ItemAmount;
                    return true;
                }
                else
                {
                    return false;
                }

            case Accessory:
                if (_AccessoryInBag.Find(x => x.thisItem == item.thisItem) is {} v4)
                {
                    count = (uint)v4.ItemAmount;
                    return true;
                }
                else
                {
                    return false;
                }

            case KeyItem:
                if (KeyItemsInBag.Find(x => x.thisItem == item.thisItem) is {} v5)
                {
                    count = (uint)v5.ItemAmount;
                    return true;
                }
                else
                {
                    return false;
                }

            case Loot:
                if (LootInBag.Find(x => x.thisItem == item.thisItem) is {} v6)
                {
                    count = (uint)v6.ItemAmount;
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
