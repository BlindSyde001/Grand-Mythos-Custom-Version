using System.Collections;
using UnityEngine;

public abstract class MenuContainer : MonoBehaviour
{
    MenuInputs _menuInputs;
    protected MenuInputs MenuInputs => _menuInputs ??= FindObjectOfType<MenuInputs>();
    protected GameManager GameManager => GameManager.Instance;
    protected InventoryManager InventoryManager => InventoryManager.Instance;

    public abstract IEnumerable Open(MenuInputs menuInputs);
    public abstract IEnumerable Close(MenuInputs menuInputs);
}