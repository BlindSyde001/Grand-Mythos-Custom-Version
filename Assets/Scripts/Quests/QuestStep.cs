using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class QuestStep : ScriptableObject
{
    [OnValueChanged(nameof(OnTitleChanged))]
    public string Title = "New Step";

    [TextArea]
    public string Description;

    [SerializeField, HideInInspector]
    Quest _quest;

    [SerializeField, HideInInspector]
    guid _guid = guid.New();

    public guid Guid => _guid;

    public Quest Quest
    {
        get => _quest;
        set
        {
            if (_quest != null)
                throw new InvalidOperationException();
            _quest = value;
        }
    }

    void OnTitleChanged()
    {
        name = $"{Quest.name} - {Title}";
    }
}