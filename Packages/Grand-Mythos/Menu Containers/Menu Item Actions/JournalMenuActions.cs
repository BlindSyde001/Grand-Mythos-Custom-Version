using System.Collections;
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
        QuestButtons.Clear();
        foreach (var quest in GameManager.DiscoveredQuests.OrderBy(x => x.name))
        {
            QuestButtons.Allocate(out var button);
            button.Text.text = quest.name;
            if (quest.Completed)
                button.Text.fontStyle = FontStyles.Strikethrough;
            button.Button.onClick.AddListener(() => SelectQuest(quest));
        }

        QuestTitle.text = "";
        StepTitle.text = "";
        Description.text = "";
        if (GameManager.DiscoveredQuests.Count > 0)
            SelectQuest(GameManager.DiscoveredQuests.OrderBy(x => x.name).First());

        gameObject.SetActive(true);

        QuickFade(gameObject, 1, menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
    }

    public override IEnumerable Close(MenuInputs menuInputs)
    {
        QuickFade(gameObject, 0, menuInputs.Speed);
        yield return new WaitForSeconds(menuInputs.Speed);
        gameObject.SetActive(false);
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

    void SelectQuest(Quest quest)
    {
        QuestTitle.text = quest.name;
        QuestTitle.fontStyle = quest.Completed ? FontStyles.Strikethrough : FontStyles.Normal;
        var currentStep = quest.Steps.FirstOrDefault(x => x.Completed == false) ?? (quest.Steps.Length > 0 ? quest.Steps[^1] : null);
        StepTitle.text = currentStep?.name ?? "";
        StepTitle.fontStyle = currentStep?.Completed == true ? FontStyles.Strikethrough : FontStyles.Normal;
        Description.text = currentStep?.Description ?? quest.Description;
    }
}
