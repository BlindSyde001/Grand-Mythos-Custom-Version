using System;
using System.Collections.Generic;
using Interactables;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu(" GrandMythos/Shop")]
public class Shop : MonoBehaviour
{
    [Header("Logic")]
    public Categories PlayersCanSellItemsOfType = ~Categories.Loot; // Everything but loot by default, loot is reserved to poachers
    public float BuyRatio = 1f;
    public float SellRatio = 0.5f;
    [TableList]
    public List<ItemWithCondition> Stock = new();

    [Header("UI")]
    [Required] public Button BuyTab;
    [Required] public Button SellTab;
    public UIElementList<ShopCategoryTab> CategoriesContainer = new();
    public UIElementList<ShopItemButton> ItemsList = new();

    [Required] public TMP_Text PlayerCredits;

    Categories _selectedCategory;

    void OnEnable()
    {
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

        var items = new List<TradeableItem>();
        if (buy)
        {
            foreach (var itemWithCondition in Stock)
            {
                if (itemWithCondition.Availability == null || itemWithCondition.Availability.Evaluate())
                    items.Add(itemWithCondition.Item);
            }
        }
        else
        {
            foreach ((TradeableItem item, uint count) in InventoryManager.Instance.Enumerate<TradeableItem>())
            {
                if ((ExtractCategory(item) & PlayersCanSellItemsOfType) == 0)
                    continue;

                for (int i = 0; i < count; i++)
                    items.Add(item);
            }
        }

        var categories = Categories.None;
        foreach (var item in items)
            categories |= ExtractCategory(item);

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
        foreach (var item in items)
        {
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
                    PlayerTryBuy(item);
                    // ReSharper disable once AccessToModifiedClosure
                    OnBuy();
                });
            else
                element.Button.onClick.AddListener(() =>
                {
                    PlayerTrySell(item);
                    ItemsList.Remove(element);
                });
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
    public struct ItemWithCondition
    {
        [Required]
        public TradeableItem Item;
        [SerializeReference]
        public ICondition Availability;
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