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

    [SerializeField]
    private int selectedList;
    [SerializeField]
    private Button SortButton;

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

    public void ShowConsumables()
    {
        itemDescriptionName.text = "";
        itemDescriptionText.text = "";
        itemDescriptionStats.text = "";
        foreach(GameObject a in ItemButtons)
        {
            a.GetComponent<ItemButtonContainer>().itemName.text = "";
            a.GetComponent<ItemButtonContainer>().itemAmount.text = "";
            a.GetComponent<ItemButtonContainer>().itemDescription = "";
            a.GetComponent<Button>().onClick.RemoveAllListeners();
        }
        for(int i = 0; i < InventoryManager.ConsumablesInBag.Count; i++)
        {
            ItemButtons[i].SetActive(true);
            ItemButtonContainer btn = ItemButtons[i].GetComponent<ItemButtonContainer>();
            
            btn.itemName.text = InventoryManager.ConsumablesInBag[i].thisItem.name;
            btn.itemAmount.text = InventoryManager.ConsumablesInBag[i].ItemAmount.ToString();
            btn.itemDescription = InventoryManager.ConsumablesInBag[i].thisItem.Description;

            btn.GetComponent<Button>().onClick.AddListener(delegate { DisplayItemDescription(btn); });
        }
        selectedList = 0;
        SortButton.onClick.RemoveAllListeners();
        SortButton.onClick.AddListener(delegate { CallSortInventory(); });
        SortButton.onClick.AddListener(delegate { ShowConsumables(); });
    }
    public void ShowEquipment()
    {
        itemDescriptionName.text = "";
        itemDescriptionText.text = "";
        itemDescriptionStats.text = "";
        foreach (GameObject a in ItemButtons)
        {
            a.GetComponent<ItemButtonContainer>().itemName.text = "";
            a.GetComponent<ItemButtonContainer>().itemAmount.text = "";
            a.GetComponent<Button>().onClick.RemoveAllListeners();
        }
        for (int i = 0; i < InventoryManager.EquipmentInBag.Count; i++)
        {
            ItemButtons[i].SetActive(true);
            ItemButtonContainer btn = ItemButtons[i].GetComponent<ItemButtonContainer>();

            btn.itemName.text = InventoryManager.EquipmentInBag[i].thisItem.name;
            btn.itemAmount.text = InventoryManager.EquipmentInBag[i].ItemAmount.ToString();
            btn.itemDescription = InventoryManager.EquipmentInBag[i].thisItem.Description;

            btn.GetComponent<Button>().onClick.AddListener(delegate { DisplayItemDescription(btn); });
        }
        selectedList = 1;
        SortButton.onClick.RemoveAllListeners();
        SortButton.onClick.AddListener(delegate { CallSortInventory(); });
        SortButton.onClick.AddListener(delegate { ShowEquipment(); });
    }
    public void ShowKeyItems()
    {
        itemDescriptionName.text = "";
        itemDescriptionText.text = "";
        itemDescriptionStats.text = "";
        foreach (GameObject a in ItemButtons)
        {
            a.GetComponent<ItemButtonContainer>().itemName.text = "";
            a.GetComponent<ItemButtonContainer>().itemAmount.text = "";
            a.GetComponent<ItemButtonContainer>().itemDescription = "";
            a.GetComponent<Button>().onClick.RemoveAllListeners();
        }
        for (int i = 0; i < InventoryManager.KeyItemsInBag.Count; i++)
        {
            ItemButtons[i].SetActive(true);
            ItemButtonContainer btn = ItemButtons[i].GetComponent<ItemButtonContainer>();

            btn.itemName.text = InventoryManager.KeyItemsInBag[i].thisItem.name;
            btn.itemAmount.text = InventoryManager.KeyItemsInBag[i].ItemAmount.ToString();
            btn.itemDescription = InventoryManager.KeyItemsInBag[i].thisItem.Description;

            btn.GetComponent<Button>().onClick.AddListener(delegate { DisplayItemDescription(btn); });
        }
        selectedList = 2;
        SortButton.onClick.RemoveAllListeners();
        SortButton.onClick.AddListener(delegate { CallSortInventory(); });
        SortButton.onClick.AddListener(delegate { ShowKeyItems(); });
    }
    public void ShowLoot()
    {
        itemDescriptionName.text = "";
        itemDescriptionText.text = "";
        itemDescriptionStats.text = "";
        foreach (GameObject a in ItemButtons)
        {
            a.GetComponent<ItemButtonContainer>().itemName.text = "";
            a.GetComponent<ItemButtonContainer>().itemAmount.text = "";
            a.GetComponent<ItemButtonContainer>().itemDescription = "";
            a.GetComponent<Button>().onClick.RemoveAllListeners();
        }
        for (int i = 0; i < InventoryManager.LootInBag.Count; i++)
        {
            ItemButtons[i].SetActive(true);
            ItemButtonContainer btn = ItemButtons[i].GetComponent<ItemButtonContainer>();

            btn.itemName.text = InventoryManager.LootInBag[i].thisItem.name;
            btn.itemAmount.text = InventoryManager.LootInBag[i].ItemAmount.ToString();
            btn.itemDescription = InventoryManager.LootInBag[i].thisItem.Description;

            btn.GetComponent<Button>().onClick.AddListener(delegate { DisplayItemDescription(btn); });
        }
        selectedList = 3;
        SortButton.onClick.RemoveAllListeners();
        SortButton.onClick.AddListener(delegate { CallSortInventory(); });
        SortButton.onClick.AddListener(delegate { ShowLoot(); });
    }
    internal void DisplayItemDescription(ItemButtonContainer data)
    {
        itemDescriptionName.text = data.itemName.text;
        itemDescriptionText.text = data.itemDescription;
    }

    public void CallSortInventory()
    {
        InventoryManager.SortInventory(selectedList);
    }
}