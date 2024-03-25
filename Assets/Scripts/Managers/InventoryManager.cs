using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine.Events;

public class InventoryManager : MonoBehaviour, ISerializationCallbackReceiver, ISaved<InventoryManager, InventoryManager.SaveV1>
{
    // VARIABLES
    public static InventoryManager Instance { get; private set; }

    Sort _lastSort;
    /// <summary>
    /// The first element in sorting order, recurse over this object's Next to iterate in order
    /// </summary>
    ItemData _first;

    [SerializeField]
    [OnValueChanged(nameof(EditorCreditsChanged))]
    int _credits = 1000;

    [SerializeField] SerializableDictionary<BaseItem, ItemData> Items = new();

    public List<ActionCondition> ConditionsAcquired;

    public int Credits
    {
        get => _credits;
        set
        {
            _credits = value;
            OnCreditsChanged?.Invoke(_credits);
        }
    }

    public UnityAction<int> OnCreditsChanged;
    public UnityAction OnItemsChanged;

    void ISerializationCallbackReceiver.OnBeforeSerialize() {}

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        foreach (var (item, data) in Items)
        {
            if (data.Item == null)
                data.Item = item;
        }

        #if UNITY_EDITOR
        if (DomainReloadHelper.LastState == DomainReloadHelper.LastPlayModeState.EnteredPlayMode)
            SortBy(Sort.Type); // Make sure that if the user is adding new items through the editor during runtime, those are sorted and part of the enumeration
        #endif
    }

    public enum Sort
    {
        Timestamp,
        Type,
        Count,
        Name,
    }

















    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        SavingSystem.TryRestore<InventoryManager, SaveV1>(this);
    }

    void OnDestroy()
    {
        SavingSystem.Unregister<InventoryManager, SaveV1>(this);
    }

    // UPDATES
    void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public void Remove(BaseItem item, uint count)
    {
        if (count == 0)
            return;

        if (Items.TryGetValue(item, out var data) == false)
            throw new ArgumentException($"No instance of {item} found in this inventory");

        if (data.Count < count)
            throw new InvalidOperationException("Cannot remove more than the inventory contains");

        data.Count -= count;
        if (data.Count == 0)
        {
            data.Previous.Next = data.Next; // Remove from the linked list
            data.Next.Previous = data.Previous;
            data.Next = null;
            data.Previous = null;
            Items.Remove(item);
        }
        else
        {
            if (_lastSort == Sort.Count)
                SortedInsertion(data);
        }
        OnItemsChanged?.Invoke();
    }

    public void AddToInventory(BaseItem item, uint count)
    {
        if (count == 0)
            return;

        if (Items.TryGetValue(item, out var data))
        {
            data.Count += count;
            if (_lastSort == Sort.Count)
                SortedInsertion(data);
        }
        else // New item
        {
            Items[item] = data = new ItemData() { Item = item, Count = count };
            SortedInsertion(data);
        }
        OnItemsChanged?.Invoke();
    }

    void SortedInsertion(ItemData insertion)
    {
        switch (_lastSort)
        {
            case Sort.Timestamp: SortedInsertion(new SortByTimestamp(), insertion, ref _first); break;
            case Sort.Type: SortedInsertion(new SortByType(), insertion, ref _first); break;
            case Sort.Count: SortedInsertion(new SortByCount(), insertion, ref _first); break;
            case Sort.Name: SortedInsertion(new SortByName(), insertion, ref _first); break;
            default: throw new NotImplementedException(_lastSort.ToString());
        }
    }

    static void SortedInsertion<T>(T comparer, ItemData insertion, ref ItemData first) where T : IComparer<ItemData>
    {
        if (insertion.Previous != null)
            insertion.Previous.Next = insertion.Next;
        if (insertion.Next != null)
            insertion.Next.Previous = insertion.Previous;
        insertion.Next = null;
        insertion.Previous = null;

        if (first == null) // No items in the linked list
        {
            first = insertion;
            return;
        }

        for (var other = first; other != null; other = other.Next)
        {
            if (comparer.Compare(insertion, other) <= 0)
            {
                var left = other.Previous;
                var middle = insertion;
                var right = other;

                if (left != null)
                    left.Next = middle;
                middle.Previous = left;
                middle.Next = right;
                right.Previous = middle;

                if (left == null) // Enqueued before the first item, change the first to this one
                    first = insertion;
                break;
            }
            else if (other.Next == null) // End of the linked list, add it at the end
            {
                other.Next = insertion;
                insertion.Previous = other;
                break;
            }
        }
    }

    public bool FindItem(BaseItem item, out uint count)
    {
        if (Items.TryGetValue(item, out var data))
        {
            count = data.Count;
            return true;
        }

        count = 0;
        return false;
    }

    public void ClearItems()
    {
        Items.Clear();
        _first = null;
    }

    public IEnumerable<(T item, uint count)> Enumerate<T>() where T : BaseItem
    {
        #warning perf: Could create an explicit struct enumerator to avoid allocation
        for (var i = _first; i != null; i = i.Next)
        {
            if (i.Item is T ofT)
                yield return (ofT, i.Count);
        }
    }

    public void SortBy(Sort sort)
    {
        _lastSort = sort;
        var sorting = Items.OrderBy(x => x.Value, sort switch
        {
            Sort.Timestamp => new SortByTimestamp(),
            Sort.Type => new SortByType(),
            Sort.Count => new SortByCount(),
            Sort.Name => new SortByName(),
            _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, null)
        });

        ItemData previous = null;
        _first = null;
        foreach (var (_, data) in sorting)
        {
            data.Next = null;
            data.Previous = previous;
            if (previous != null)
                previous.Next = data;

            previous = data;

            _first ??= data;
        }
    }

    void EditorCreditsChanged()
    {
        if (Application.isPlaying)
            OnCreditsChanged?.Invoke(_credits);
    }

    struct SortByTimestamp : IComparer<ItemData>
    {
        public int Compare(ItemData x, ItemData y) => x.Timestamp.CompareTo(y.Timestamp);
    }

    struct SortByType : IComparer<ItemData>
    {
        public int Compare(ItemData x, ItemData y)
        {
            var xGuid = x.Item.GetType().GUID;
            var yGuid = y.Item.GetType().GUID;
            if (xGuid != yGuid)
                return xGuid.CompareTo(yGuid);
            return x.Timestamp.CompareTo(y.Timestamp);
        }
    }

    struct SortByCount : IComparer<ItemData>
    {
        public int Compare(ItemData x, ItemData y)
        {
            if (x.Count != y.Count)
                return x.Count.CompareTo(y.Count);
            return x.Timestamp.CompareTo(y.Timestamp);
        }
    }

    struct SortByName : IComparer<ItemData>
    {
        public int Compare(ItemData x, ItemData y)
        {
            var nameComp = string.Compare(x.Item.name, y.Item.name, StringComparison.Ordinal);
            if (nameComp != 0)
                return nameComp;
            return x.Timestamp.CompareTo(y.Timestamp);
        }
    }

    [Serializable]
    public class ItemData
    {
        [Required]
        public BaseItem Item;
        public uint Count = 1;
        public DateTime Timestamp = DateTime.Now;
        [NonSerialized]
        public ItemData Previous, Next;
    }

    [Serializable] public struct SaveV1 : ISaveHandler<InventoryManager>
    {
        public int Credits;
        public Sort LastSort;
        public SavedItemData[] Items;
        public List<guid> Conditions;


        public uint Version => 1;

        public void Transfer(InventoryManager source, SavingSystem.Transfer transfer)
        {
            transfer.Value(ref Credits, ref source._credits);
            transfer.Value(ref LastSort, ref source._lastSort);
            transfer.Identifiables<List<guid>, List<ActionCondition>, ActionCondition>(ref Conditions, ref source.ConditionsAcquired);

            if (transfer == SavingSystem.Transfer.PullFromSource)
            {
                Items = source.Items.Select(x => new SavedItemData
                {
                    Item = x.Value.Item.Guid,
                    Count = x.Value.Count,
                    Timestamp = x.Value.Timestamp
                }).ToArray();
            }
            else
            {
                source.Items = new();
                foreach (var x in Items)
                {
                    if (IdentifiableDatabase.TryGet(x.Item, out BaseItem item))
                    {
                        var next = new ItemData { Item = item, Count = x.Count, Timestamp = x.Timestamp };
                        source.Items.Add(item, next);
                    }
                    else
                    {
                        Debug.LogError($"Could not find item with id '{x.Item}' in the database");
                    }
                }

                source.SortBy(LastSort);
            }
        }

        [Serializable]
        public struct SavedItemData
        {
            public guid Item;
            public uint Count;
            public DateTime Timestamp;
        }
    }

    public guid UniqueConstID => Guid;

    public static readonly guid Guid  = new guid("51d6e0d4-8916-47a6-b18e-6d16ec62a723");

    static InventoryManager()
    {
        DomainReloadHelper.BeforeReload += helper => helper.InventoryInstance = Instance;
        DomainReloadHelper.AfterReload += helper => Instance = helper.InventoryInstance;
    }
}

public partial class DomainReloadHelper
{
    public InventoryManager InventoryInstance;
}