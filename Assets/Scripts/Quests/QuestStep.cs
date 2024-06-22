using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class QuestStep : ScriptableObject
{
    [OnValueChanged(nameof(FixName))]
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

    void FixName()
    {
        name = Title;
    }

    public bool Completed
    {
        get => GameManager.Instance?.CompletedSteps.Contains(this) ?? false;
        set
        {
            if (value)
                GameManager.Instance.CompletedSteps.Add(this);
            else
                GameManager.Instance.CompletedSteps.Remove(this);
        }
    }

#if UNITY_EDITOR
    [ShowIf(nameof(_inEditModeAndNotCompleted)), Button]
    void ForceCompletion() => Completed = true;

    [ShowIf(nameof(_inEditModeAndCompleted)), Button]
    void ForceRemoveCompletion() => Completed = false;

    static bool _inEditMode;
    bool _inEditModeAndCompleted => _inEditMode && Completed;
    bool _inEditModeAndNotCompleted => _inEditMode && Completed == false;

    static QuestStep()
    {
        UnityEditor.EditorApplication.playModeStateChanged += change =>
        {
            switch (change)
            {
                case UnityEditor.PlayModeStateChange.EnteredEditMode:
                case UnityEditor.PlayModeStateChange.ExitingEditMode:
                    _inEditMode = false;
                    break;
                case UnityEditor.PlayModeStateChange.EnteredPlayMode:
                case UnityEditor.PlayModeStateChange.ExitingPlayMode:
                    _inEditMode = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(change), change, null);
            }
        };
    }
#endif
}