using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class ItemMenuActions : MenuContainer
{
    public UIElementList<ItemButtonContainer> ItemUI = new(){ Template = null! };

    public required TextMeshProUGUI itemDescriptionName;
    public required TextMeshProUGUI itemDescriptionText;
    public required TextMeshProUGUI itemDescriptionStats;

    public required GameObject PartyDisplay;
    public UIElementList<PartyContainer> CharactersUI = new(){ Template = null! };

    IFilter? _filter;

    // METHODS
    public override IEnumerable Open(MenuInputs menuInputs)
    {
        PartyDisplay.SetActive(false);
        gameObject.SetActive(true);
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-470, 200, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(-470, -250, 0),  menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(480, 450, 0), menuInputs.Speed);
        gameObject.transform.GetChild(3).DOLocalMove(new Vector3(480, gameObject.transform.GetChild(3).localPosition.y, 0), menuInputs.Speed);
        gameObject.transform.GetChild(4).DOLocalMove(new Vector3(-400, 480, 0), menuInputs.Speed);
        ShowConsumables();
        yield return new WaitForSeconds(menuInputs.Speed);
    }
    public override IEnumerable Close(MenuInputs menuInputs)
    {
        gameObject.transform.GetChild(0).DOLocalMove(new Vector3(-1450, 200, 0), menuInputs.Speed);
        gameObject.transform.GetChild(1).DOLocalMove(new Vector3(-1450, -250, 0),  menuInputs.Speed);
        gameObject.transform.GetChild(2).DOLocalMove(new Vector3(1450, 450, 0), menuInputs.Speed);
        gameObject.transform.GetChild(3).DOLocalMove(new Vector3(1450, gameObject.transform.GetChild(3).localPosition.y, 0), menuInputs.Speed);
        gameObject.transform.GetChild(4).DOLocalMove(new Vector3(-400, 600, 0), menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
        gameObject.SetActive(false);
    }

    public void ShowConsumables()
    {
        Show<Consumable>(consumable =>
        {
            PartyDisplay.SetActive(true);

            CharactersUI.Clear();
            foreach (var hero in GameManager.AllHeroes)
            {
                CharactersUI.Allocate(out var element);
                
                element.displayName.text = hero.name;
                element.displayBanner.sprite = hero.Banner;
                element.displayLevel.text = hero.Level.ToString();

                element.displayEXPBar.fillAmount = (float)(hero.Experience - hero.PrevExperienceThreshold) /
                                                   (hero.ExperienceThreshold - hero.PrevExperienceThreshold);

                element.displayHP.text = $"{hero.CurrentHP} / {hero.EffectiveStats.HP}";
                element.ChangeOrderButton.onClick.RemoveAllListeners();
                element.ChangeOrderButton.onClick.AddListener(() =>
                {
                    consumable.Perform(new CharacterTemplate[]{ hero }, new EvaluationContext(hero));
                    PartyDisplay.SetActive(false);
                });
            }
        });
    }

    public void ShowEquipment() => Show<Equipment>();

    public void ShowKeyItems() => Show<KeyItem>();

    public void ShowLoot() => Show<Loot>();

    public void Show<T>(Action<T>? onClick = null) where T : BaseItem
    {
        PartyDisplay.SetActive(false);

        itemDescriptionName.text = "";
        itemDescriptionText.text = "";
        itemDescriptionStats.text = "";
        ItemUI.Clear();

        foreach (var (item, count) in InventoryManager.Enumerate<T>())
        {
            ItemUI.Allocate(out var btn);

            btn.ItemName.text = item.name;
            btn.ItemAmount.text = count.ToString();
            btn.itemDescription = item.Description;

            btn.Button.onClick.RemoveAllListeners();
            btn.Button.onClick.AddListener(() =>
            {
                DisplayItemDescription(btn);
                onClick?.Invoke(item);
            });
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