using System.Collections;
using UnityEngine;

public abstract class MenuContainer : MonoBehaviour
{
    protected MenuInputs MenuInputs { get; private set; }
    protected GameManager GameManager { get; private set; }
    protected InventoryManager InventoryManager { get; private set; }

    protected virtual void OnEnable()
    {
        MenuInputs = FindObjectOfType<MenuInputs>();
        GameManager = GameManager._instance;
        InventoryManager = InventoryManager._instance;
    }

    public abstract IEnumerable Open(MenuInputs menuInputs);
    public abstract IEnumerable Close(MenuInputs menuInputs);
}