using System;
using System.Collections.Generic;
using System.Linq;
using Interactables;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu(" GrandMythos/Shop")]
public class Shop : MonoBehaviour, IInteractionSource
{
    [Header("Logic")]
    public Categories PlayersCanSellItemsOfType = ~Categories.Loot; // Everything but loot by default, loot is reserved to poachers
    [Tooltip("Multiplier on the base cost of items when the player buys items from this shop, PlayerMoney -= ItemCost * BuyRatio")]
    public float BuyRatio = 1f;
    [Tooltip("Multiplier on the amount received from items the player sells to the shop, PlayerMoney += ItemCost * SellRatio")]
    public float SellRatio = 0.5f;
    [TableList]
    public List<Transaction> Stock = new();

    [Header("UI")]
    public required Button BuyTab;
    public required Button SellTab;
    public UIElementList<ShopCategoryTab> CategoriesContainer = new(){ Template = null! };
    public UIElementList<ShopItemButton> ItemsList = new(){ Template = null! };

    public required TMP_Text PlayerCredits;
    public required TMP_Text ItemDescription;

    Categories _selectedCategory;

    void OnEnable()
    {
        if (InventoryManager.Instance == null!)
        {
            Debug.LogWarning($"No {typeof(InventoryManager)} in scene, disabling the shop");
            gameObject.SetActive(false);
            return;
        }

        ItemDescription.text = "";
        BuyTab.onClick.RemoveListener(OnClickBuy);
        SellTab.onClick.RemoveListener(OnClickSell);

        BuyTab.onClick.AddListener(OnClickBuy);
        SellTab.onClick.AddListener(OnClickSell);

        RefreshUI(true);
        InputManager.PushGameState(GameState.Menu, this);
    }

    void OnDisable()
    {
        InputManager.PopGameState(this);
    }

    void Update()
    {
        if (int.TryParse(PlayerCredits.text, out int c) == false || c != InventoryManager.Instance.Credits)
        {
            PlayerCredits.text = InventoryManager.Instance.Credits.ToString();
        }
    }

    void OnClickBuy()
    {
        RefreshUI(true);
    }

    void OnClickSell()
    {
        RefreshUI(false);
    }

    void RefreshUI(bool buy)
    {
        SellTab.gameObject.SetActive(PlayersCanSellItemsOfType != 0);
        BuyTab.gameObject.SetActive(Stock.Count > 0);

        var itemsAndCondition = new List<Transaction>();
        if (buy)
        {
            foreach (var itemWithCondition in Stock)
            {
                if (itemWithCondition.Availability == null || itemWithCondition.Availability.Evaluate())
                    itemsAndCondition.Add(itemWithCondition);
            }
        }
        else
        {
            foreach ((TradeableItem item, uint count) in InventoryManager.Instance.Enumerate<TradeableItem>())
            {
                if ((ExtractCategory(item) & PlayersCanSellItemsOfType) == 0)
                    continue;

                for (int i = 0; i < count; i++)
                    itemsAndCondition.Add(new(){ Item = item, Availability = null, OnTransaction = null });
            }
        }

        var categories = Categories.None;
        foreach (var transaction in itemsAndCondition)
            categories |= ExtractCategory(transaction.Item);

        CategoriesContainer.Clear();
        Categories firstValidCategory = 0;
        for (Categories i = (Categories)1; i <= categories; i = (Categories)((int)i << 1))
        {
            if ((i & categories) == 0)
                continue;

            if (firstValidCategory == 0)
                firstValidCategory = i;

            CategoriesContainer.Allocate(out var element);
            var category = i;
            element.Label.text = category.ToString();
            element.Button.onClick.RemoveAllListeners();
            element.Button.onClick.AddListener(() => ChangeToCategory(category, buy));
        }

        if (_selectedCategory == 0 || (_selectedCategory & categories) == 0)
            _selectedCategory = firstValidCategory;

        Action OnBuy = () => { };
        ItemsList.Clear();
        foreach (var transaction in itemsAndCondition)
        {
            var item = transaction.Item;
            if ((ExtractCategory(item) & _selectedCategory) == 0)
                continue;

            ItemsList.Allocate(out var element);
            element.Label.text = item.name;
            element.Cost.text = ((int)(item.Cost * (buy ? BuyRatio : SellRatio))).ToString();
            if (buy)
                element.Button.interactable = InventoryManager.Instance.Credits >= item.Cost;
            else
                element.Button.interactable = true;
            OnBuy += () => { element.Button.interactable = InventoryManager.Instance.Credits >= item.Cost; };
            element.Button.onClick.RemoveAllListeners();
            if (buy)
                element.Button.onClick.AddListener(() =>
                {
                    if ((transaction.Availability is null || transaction.Availability.Evaluate()) && PlayerTryBuy(item))
                    {
                        var controller = OverworldPlayerController.Instances.First();
                        if (transaction.OnTransaction is not null && item.OnPlayerSoldItem is not null)
                        {
                            var multi = new MultiInteraction
                            {
                                Array = new []{ transaction.OnTransaction, item.OnPlayerSoldItem },
                                Execution = MultiInteraction.Mode.Sequentially,
                            };
                            controller.PlayInteraction(this, multi);
                        }
                        else if (item.OnPlayerSoldItem is not null)
                            controller.PlayInteraction(this, item.OnPlayerSoldItem);
                        else if (transaction.OnTransaction is not null)
                            controller.PlayInteraction(this, transaction.OnTransaction);
                    }

                    if (transaction.Availability is not null && transaction.Availability.Evaluate() == false)
                    {
                        var selection = EventSystem.current.currentSelectedGameObject;
                        if (selection == element.Button.gameObject)
                        {
                            // Change selection to another element
                            var arr = ItemsList.ToArray();
                            var i = Array.IndexOf(arr, element);
                            if (i + 1 < arr.Length)
                                arr[i+1].Button.Select();
                            else if (i - 1 >= 0)
                                arr[i-1].Button.Select();
                        }
                        ItemsList.Remove(element);
                    }

                    // ReSharper disable once AccessToModifiedClosure
                    OnBuy();
                });
            else
                element.Button.onClick.AddListener(() =>
                {
                    PlayerTrySell(item);
                    var selection = EventSystem.current.currentSelectedGameObject;
                    if (selection == element.Button.gameObject)
                    {
                        // Change selection to another element
                        var arr = ItemsList.ToArray();
                        var i = Array.IndexOf(arr, element);
                        if (i + 1 < arr.Length)
                            arr[i+1].Button.Select();
                        else if (i - 1 >= 0)
                            arr[i-1].Button.Select();
                    }
                    ItemsList.Remove(element);
                });

            var onHover = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            onHover.callback.AddListener(_ => ItemDescription.text = item.Description);
            var onSelect = new EventTrigger.Entry { eventID = EventTriggerType.Select };
            onSelect.callback.AddListener(_ => ItemDescription.text = item.Description);

            if (element.Button.gameObject.TryGetComponent(out EventTrigger trigger) == false)
                trigger = element.Button.gameObject.AddComponent<EventTrigger>();

            trigger.triggers.Clear();
            trigger.triggers.Add(onHover);
            trigger.triggers.Add(onSelect);
        }
    }

    void ChangeToCategory(Categories categories, bool buy)
    {
        _selectedCategory = categories;
        RefreshUI(buy);
    }

    bool PlayerTrySell(TradeableItem item)
    {
        if (InventoryManager.Instance.FindItem(item, out var count) && count > 0)
        {
            InventoryManager.Instance.Remove(item, 1);
            var gain = (int)(item.Cost * SellRatio);
            InventoryManager.Instance.Credits += gain;
            return true;
        }

        return false;
    }

    bool PlayerTryBuy(TradeableItem item)
    {
        var cost = (int)(item.Cost * BuyRatio);
        if (InventoryManager.Instance.Credits < cost)
            return false;

        InventoryManager.Instance.AddToInventory(item, 1);
        InventoryManager.Instance.Credits -= cost;
        return true;
    }

    [Serializable]
    public struct Transaction
    {
        public required TradeableItem Item;
        [SerializeReference, Tooltip("Condition for this item to become available, None means it is always available")]
        public ICondition? Availability;
        [SerializeReference, Tooltip("Interaction occuring once this item is bought")]
        public IInteraction? OnTransaction;
    }

    [Flags]
    public enum Categories
    {
        None        = 0,
        Consumables = 0b0000_0001,
        Weapons     = 0b0000_0010,
        Armours     = 0b0000_0100,
        Accessories = 0b0000_1000,
        Loot        = 0b0001_0000,
        Any         = ~0,
    }

    static Categories ExtractCategory(BaseItem item)
    {
        Categories output = 0;

        if (item is Consumable)
            output |= Categories.Consumables;

        if (item is Weapon)
            output |= Categories.Weapons;

        if (item is Armour)
            output |= Categories.Armours;

        if (item is Accessory)
            output |= Categories.Accessories;

        if (item is Loot)
            output |= Categories.Loot;

        return output;
    }
}