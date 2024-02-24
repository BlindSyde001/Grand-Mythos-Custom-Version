using System;
using System.Collections.Generic;
using UnityEngine;

public interface IInventory
{
    public bool HasItem(BaseItem item, out uint count);
    public void RemoveItem(BaseItem item, uint count);
    public IEnumerable<(BaseItem item, uint count)> Items();
}

[Serializable]
public class InlineInventory : IInventory
{
    public SerializableDictionary<BaseItem, uint> Items = new();

    public bool HasItem(BaseItem item, out uint count)
    {
        if (Items.TryGetValue(item, out count))
        {
            Debug.Assert(count != 0);
            return true;
        }

        return false;
    }

    public void RemoveItem(BaseItem item, uint count)
    {
        if (Items.TryGetValue(item, out uint currentCount))
        {
            if (currentCount > count)
                Items[item] = currentCount - count;
            else
                Items.Remove(item);
        }
    }

    IEnumerable<(BaseItem item, uint count)> IInventory.Items()
    {
        foreach (var kvp in Items)
        {
            yield return (kvp.Key, kvp.Value);
        }
    }
}

[Serializable]
public class ProxyForInventoryComponent : IInventory
{
    public InventoryManager Inventory;

    public bool HasItem(BaseItem item, out uint count)
    {
        return Inventory.FindItem(item, out count);
    }

    public void RemoveItem(BaseItem item, uint count)
    {
        Inventory.Remove(item, count);
    }

    public IEnumerable<(BaseItem item, uint count)> Items()
    {
        foreach (var (item, count) in Inventory.Enumerate<BaseItem>())
            yield return (item, count);
    }
}