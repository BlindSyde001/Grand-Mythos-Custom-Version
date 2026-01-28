using System;
using Interactables;
using Sirenix.OdinInspector;
using UnityEngine;

public class QuestBoard : MonoBehaviour
{
    [TableList]
    public required ConditionalQuest[] Quests;
    public required UIElementList<QuestItem> UIItems;

    void OnEnable()
    {
        if (GameManager.Instance == null!)
        {
            Debug.LogWarning($"No {typeof(GameManager)} in scene, disabling the shop");
            gameObject.SetActive(false);
            return;
        }

        RefreshDisplay();

        InputManager.PushGameState(GameState.Menu, this);
    }

    void RefreshDisplay()
    {
        UIItems.Clear();
        foreach (var conditionalQuest in Quests)
        {
            if (conditionalQuest.Condition != null && conditionalQuest.Condition.Evaluate() == false)
                continue;

            UIItems.Allocate(out var questItem);
            questItem.Button.interactable = conditionalQuest.Quest.Discovered == false;
            questItem.Button.onClick.AddListener(() =>
            {
                conditionalQuest.Quest.Discovered = true;
                RefreshDisplay();
            });
            questItem.Title.text = conditionalQuest.Quest.name;
            questItem.Description.text = conditionalQuest.Quest.Description;
        }
    }

    void OnDisable()
    {
        InputManager.PopGameState(this);
    }

    [Serializable]
    public struct ConditionalQuest
    {
        public required Quest Quest;
        [SerializeReference, Tooltip("If this is set to a condition, that condition must be true for the quest to show up")]
        public ICondition? Condition;
    }
}