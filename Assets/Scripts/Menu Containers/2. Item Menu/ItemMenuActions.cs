using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class ItemMenuActions : MonoBehaviour
{
    // VARIABLES
    private MenuInputs menuInputs;
    private InputManager inputManager;
    private InventoryManager inventoryManager;

    public List<GameObject> ItemButtons;

    public TextMeshProUGUI itemDescriptionName;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemDescriptionStats;

    [SerializeField]
    private int selectedList;
    [SerializeField]
    private Button SortButton;

    // UPDATES
    private void Start()
    {
        menuInputs = FindObjectOfType<MenuInputs>();
        inputManager = InputManager._instance;
        inventoryManager = InventoryManager._instance;
    }

    // METHODS
    internal IEnumerator ItemMenuOpen()
    {
        if (!menuInputs.coroutineRunning)
        {
            yield return new WaitForSeconds(menuInputs.speed);
            inputManager.MenuItems[1].SetActive(true);
            inputManager.MenuItems[1].transform.GetChild(0).DOLocalMove(new Vector3(-800, 480, 0), menuInputs.speed);
            inputManager.MenuItems[1].transform.GetChild(1).DOLocalMove(new Vector3(-470, 200, 0), menuInputs.speed);
            inputManager.MenuItems[1].transform.GetChild(2).DOLocalMove(new Vector3(-470, -250, 0),  menuInputs.speed);
            inputManager.MenuItems[1].transform.GetChild(3).DOLocalMove(new Vector3(480, 450, 0), menuInputs.speed);
            inputManager.MenuItems[1].transform.GetChild(4).DOLocalMove(new Vector3(480, -50, 0), menuInputs.speed);
            inputManager.MenuItems[1].transform.GetChild(5).DOLocalMove(new Vector3(-400, 480, 0), menuInputs.speed);
            ShowConsumables();
        }
    }
    internal IEnumerator ItemMenuClose(bool closeAllOverride)
    {
        if (!menuInputs.coroutineRunning)
        {
            menuInputs.coroutineRunning = true;
            inputManager.MenuItems[1].transform.GetChild(0).DOLocalMove(new Vector3(-1750, 480, 0), menuInputs.speed);
            inputManager.MenuItems[1].transform.GetChild(1).DOLocalMove(new Vector3(-1450, 200, 0), menuInputs.speed);
            inputManager.MenuItems[1].transform.GetChild(2).DOLocalMove(new Vector3(-1450, -250, 0),  menuInputs.speed);
            inputManager.MenuItems[1].transform.GetChild(3).DOLocalMove(new Vector3(1450, 450, 0), menuInputs.speed);
            inputManager.MenuItems[1].transform.GetChild(4).DOLocalMove(new Vector3(1450, -50, 0), menuInputs.speed);
            inputManager.MenuItems[1].transform.GetChild(5).DOLocalMove(new Vector3(-400, 600, 0), menuInputs.speed);
            yield return new WaitForSeconds(menuInputs.speed);
            inputManager.MenuItems[1].SetActive(false);
            menuInputs.coroutineRunning = false;
        }
        if (!closeAllOverride)
        {
            menuInputs.startMenuActions.StartMenuOpen();
            yield return new WaitForSeconds(menuInputs.speed);
            menuInputs.currentMenuOpen = 0;
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
        for(int i = 0; i < inventoryManager.ConsumablesInBag.Count; i++)
        {
            ItemButtons[i].SetActive(true);
            ItemButtonContainer btn = ItemButtons[i].GetComponent<ItemButtonContainer>();
            
            btn.itemName.text = inventoryManager.ConsumablesInBag[i].thisItem._ItemName;
            btn.itemAmount.text = inventoryManager.ConsumablesInBag[i].ItemAmount.ToString();
            btn.itemDescription = inventoryManager.ConsumablesInBag[i].thisItem._ItemDescription;

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
        for (int i = 0; i < inventoryManager.EquipmentInBag.Count; i++)
        {
            ItemButtons[i].SetActive(true);
            ItemButtonContainer btn = ItemButtons[i].GetComponent<ItemButtonContainer>();

            btn.itemName.text = inventoryManager.EquipmentInBag[i].thisItem._ItemName;
            btn.itemAmount.text = inventoryManager.EquipmentInBag[i].ItemAmount.ToString();
            btn.itemDescription = inventoryManager.EquipmentInBag[i].thisItem._ItemDescription;

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
        for (int i = 0; i < inventoryManager.KeyItemsInBag.Count; i++)
        {
            ItemButtons[i].SetActive(true);
            ItemButtonContainer btn = ItemButtons[i].GetComponent<ItemButtonContainer>();

            btn.itemName.text = inventoryManager.KeyItemsInBag[i].thisItem._ItemName;
            btn.itemAmount.text = inventoryManager.KeyItemsInBag[i].ItemAmount.ToString();
            btn.itemDescription = inventoryManager.KeyItemsInBag[i].thisItem._ItemDescription;

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
        for (int i = 0; i < inventoryManager.LootInBag.Count; i++)
        {
            ItemButtons[i].SetActive(true);
            ItemButtonContainer btn = ItemButtons[i].GetComponent<ItemButtonContainer>();

            btn.itemName.text = inventoryManager.LootInBag[i].thisItem._ItemName;
            btn.itemAmount.text = inventoryManager.LootInBag[i].ItemAmount.ToString();
            btn.itemDescription = inventoryManager.LootInBag[i].thisItem._ItemDescription;

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
        inventoryManager.SortInventory(selectedList);
    }
}