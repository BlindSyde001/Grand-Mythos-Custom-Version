using System.Collections;
using UnityEngine;
using DG.Tweening;
using JetBrains.Annotations;
using TMPro;

public class ItemMenuActions : MenuContainer
{
    public UIElementList<ItemButtonContainer> ItemUI = new();

    public TextMeshProUGUI itemDescriptionName;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemDescriptionStats;

    [CanBeNull] IFilter _filter;

    // METHODS
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        gameObject.SetActive(true);
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(-470, 200, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(-470, -250, 0),  menuInputs.Speed);
        gameObject.transform.GetChild(3).DOLocalMove(new Vector3(480, 450, 0), menuInputs.Speed);
        gameObject.transform.GetChild(4).DOLocalMove(new Vector3(480, gameObject.transform.GetChild(4).localPosition.y, 0), menuInputs.Speed);
        gameObject.transform.GetChild(5).DOLocalMove(new Vector3(-400, 480, 0), menuInputs.Speed);
        ShowConsumables();
        yield return new WaitForSeconds(menuInputs.Speed);
    }
    public override IEnumerable Close(MenuInputs menuInputs)
    {
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-1750, 480, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(-1450, 200, 0), menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(-1450, -250, 0),  menuInputs.Speed);
        gameObject.transform.GetChild(3).DOLocalMove(new Vector3(1450, 450, 0), menuInputs.Speed);
        gameObject.transform.GetChild(4).DOLocalMove(new Vector3(1450, gameObject.transform.GetChild(4).localPosition.y, 0), menuInputs.Speed);
        gameObject.transform.GetChild(5).DOLocalMove(new Vector3(-400, 600, 0), menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
        gameObject.SetActive(false);
    }

    public void ShowConsumables() => Show<Consumable>();
    public void ShowEquipment() => Show<Equipment>();

    public void ShowKeyItems() => Show<KeyItem>();

    public void ShowLoot() => Show<Loot>();

    public void Show<T>() where T : BaseItem
    {
        itemDescriptionName.text = "";
        itemDescriptionText.text = "";
        itemDescriptionStats.text = "";
        ItemUI.Clear();

        foreach (var (item, count) in InventoryManager.Enumerate<T>())
        {
            ItemUI.Allocate(out var element);
            var btn = element;

            btn.ItemName.text = item.name;
            btn.ItemAmount.text = count.ToString();
            btn.itemDescription = item.Description;

            btn.Button.onClick.RemoveAllListeners();
            btn.Button.onClick.AddListener(() => DisplayItemDescription(btn));
        }

        _filter = new FilterOf<T>();
    }

    internal void DisplayItemDescription(ItemButtonContainer data)
    {
        itemDescriptionName.text = data.ItemName.text;
        itemDescriptionText.text = data.itemDescription;
    }

    public void SortByName()
    {
        InventoryManager.SortBy(InventoryManager.Sort.Name);
        if (_filter is not null)
            _filter.Show(this);
        else
            ShowConsumables();
    }

    class FilterOf<T> : IFilter where T : BaseItem
    {
        public void Show(ItemMenuActions actions) => actions.Show<T>();
    }

    interface IFilter
    {
        public void Show(ItemMenuActions actions);
    }
}