using System.Collections;

public abstract class MenuContainer : ReloadableBehaviour
{
    protected MenuInputs MenuInputs { get; private set; }
    protected GameManager GameManager => GameManager.Instance;
    protected InventoryManager InventoryManager => InventoryManager.Instance;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    protected override void OnEnabled(bool afterDomainReload)
    {
        MenuInputs = FindObjectOfType<MenuInputs>();
    }

    protected override void OnDisabled(bool afterDomainReload) {}

    public abstract IEnumerable Open(MenuInputs menuInputs);
    public abstract IEnumerable Close(MenuInputs menuInputs);
}