using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class ItemMenuActions : MenuContainer
{
    public List<GameObject> ItemButtons;

    public TextMeshProUGUI itemDescriptionName;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemDescriptionStats;

    [SerializeField] int selectedList;
    [SerializeField] Button SortButton;

    // METHODS
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        if (!menuInputs.coroutineRunning)
        {
            yield return new WaitForSeconds(menuInputs.speed);
            gameObject.SetActive(true);
            gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.speed);
            gameObject.transform.GetChild(1).DOLocalMove(new Vector3(-470, 200, 0), menuInputs.speed);
            gameObject.transform.GetChild(2).DOLocalMove(new Vector3(-470, -250, 0),  menuInputs.speed);
            gameObject.transform.GetChild(3).DOLocalMove(new Vector3(480, 450, 0), menuInputs.speed);
            gameObject.transform.GetChild(4).DOLocalMove(new Vector3(480, -50, 0), menuInputs.speed);
            gameObject.transform.GetChild(5).DOLocalMove(new Vector3(-400, 480, 0), menuInputs.speed);
            ShowConsumables();
        }
    }
    public override IEnumerable Close(MenuInputs menuInputs)
    {
        if (!menuInputs.coroutineRunning)
        {
            menuInputs.coroutineRunning = true;
            gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-1750, 480, 0), menuInputs.speed);
            gameObject.transform.GetChild(1).DOLocalMove(new Vector3(-1450, 200, 0), menuInputs.speed);
            gameObject.transform.GetChild(2).DOLocalMove(new Vector3(-1450, -250, 0),  menuInputs.speed);
            gameObject.transform.GetChild(3).DOLocalMove(new Vector3(1450, 450, 0), menuInputs.speed);
            gameObject.transform.GetChild(4).DOLocalMove(new Vector3(1450, -50, 0), menuInputs.speed);
            gameObject.transform.GetChild(5).DOLocalMove(new Vector3(-400, 600, 0), menuInputs.speed);
            yield return new WaitForSeconds(menuInputs.speed);
            gameObject.SetActive(false);
            menuInputs.coroutineRunning = false;
        }
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
        foreach(GameObject a in ItemButtons)
        {
            var btn = a.GetComponent<ItemButtonContainer>();
            btn.itemName.text = "";
            btn.itemAmount.text = "";
            btn.itemDescription = "";
            a.GetComponent<Button>().onClick.RemoveAllListeners();
        }

        int i = 0;
        foreach (var (item, count) in InventoryManager.Enumerate<T>())
        {
            ItemButtons[i].SetActive(true);
            var btn = ItemButtons[i].GetComponent<ItemButtonContainer>();

            btn.itemName.text = item.name;
            btn.itemAmount.text = count.ToString();
            btn.itemDescription = item.Description;

            btn.GetComponent<Button>().onClick.AddListener(() => DisplayItemDescription(btn));
            i++;
        }
        selectedList = 0;
        SortButton.onClick.RemoveAllListeners();
        SortButton.onClick.AddListener(CallSortInventory);
        SortButton.onClick.AddListener(ShowConsumables);
    }

    internal void DisplayItemDescription(ItemButtonContainer data)
    {
        itemDescriptionName.text = data.itemName.text;
        itemDescriptionText.text = data.itemDescription;
    }

    public void CallSortInventory()
    {
        InventoryManager.SortBy(InventoryManager.Sort.Name);
    }
}