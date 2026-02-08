using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JournalMenuActions : MenuContainer
{
    public UIElementList<QuestButton> QuestButtons = new(){ Template = null! };
    public required TMP_Text QuestTitle, StepTitle, Description;

    public override IEnumerable Open(MenuInputs menuInputs)
    {
        DisplayQuests();

        gameObject.SetActive(true);

        QuickFade(gameObject, 1, menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
    }

    private void DisplayT<T>(IEnumerable<T> items, Func<T, bool> strikenThrough, Action<T> onSelect) where T : ScriptableObject
    {
        QuestButtons.Clear();
        foreach (var quest in items.OrderBy(x => x.name))
        {
            QuestButtons.Allocate(out var button);
            button.Text.text = quest.name;
            button.Text.fontStyle = strikenThrough(quest) ? FontStyles.Strikethrough : FontStyles.Normal;
            button.Button.onClick.AddListener(() => onSelect(quest));
        }

        QuestTitle.text = "";
        StepTitle.text = "";
        Description.text = "";
        if (items.Any())
            onSelect(items.OrderBy(x => x.name).First());
    }

    public override IEnumerable Close(MenuInputs menuInputs)
    {
        QuickFade(gameObject, 0, menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
        gameObject.SetActive(false);
    }
    
    public void DisplayQuests()
    {
        DisplayT(GameManager.DiscoveredQuests, quest => quest.Completed, quest =>
        {
            QuestTitle.text = quest.name;
            QuestTitle.fontStyle = quest.Completed ? FontStyles.Strikethrough : FontStyles.Normal;

            var step = quest.Steps.FirstOrDefault(x => GameManager.CompletedSteps.Contains(x) == false) ?? quest.Steps.LastOrDefault();

            if (step is not null)
            {
                StepTitle.text = step.Title;
                StepTitle.fontStyle = GameManager.CompletedSteps.Contains(step) ? FontStyles.Strikethrough : FontStyles.Normal;
                Description.text = step.Description;
            }
            else
            {
                StepTitle.text = "";
                StepTitle.fontStyle = FontStyles.Normal;
                Description.text = "";
            }
        });
    }
    
    public void DisplayPeople()
    {
        DisplayT(Array.Empty<Quest>(), character => false, character =>
        {
        });
    }

    public void DisplayEvents()
    {
        DisplayT(Array.Empty<Quest>(), character => false, character =>
        {
        });
    }

    public void DisplayBestiary()
    {
        DisplayT(GameManager.HostileStats.Select(x =>
            {
                if (IdentifiableDatabase.TryGet(x.Key, out CharacterTemplate? character))
                    return character!;
                throw new Exception(x.Key.ToString());
            }), character => false, character =>
        {
            var stats = GameManager.HostileStats[character!.Guid];

            QuestTitle.text = character.name;
            QuestTitle.fontStyle = FontStyles.Normal;

            StepTitle.text = "";
            StepTitle.fontStyle = FontStyles.Normal;
            Description.text = @$"
Amount Defeated: {stats.AmountDefeated}
Location: {character.BestiaryLocationDescription}
Drops:
{character.DropItems.Select(x => $"{x.Item.name}: {x.Count}x {x.DropRatePercent}%").StringSequence("\n")}
".Trim();
            
        });
    }

    public void DisplayItems()
    {
        DisplayT(InventoryManager.Instance.Enumerate<BaseItem>().Select(x => x.item), item => false, item =>
        {
            QuestTitle.text = item.name;
            QuestTitle.fontStyle = FontStyles.Normal;

            StepTitle.text = "";
            StepTitle.fontStyle = FontStyles.Normal;
            Description.text = item.Description;
        });
    }


    void QuickFade(GameObject target, float goalAlpha, float speed)
    {
        foreach (var image in target.GetComponentsInChildren<Graphic>())
        {
            if (image.isActiveAndEnabled == false || image.gameObject.activeInHierarchy == false)
                continue;
            var col = image.color;
            col.a = 1-goalAlpha;
            image.color = col;
            image.DOFade(goalAlpha, speed);
        }
    }
}