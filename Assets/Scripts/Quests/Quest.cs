using Sirenix.OdinInspector;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[CreateAssetMenu(menuName = "Quest")]
public class Quest : IdentifiableScriptableObject
{
    [TextArea, Space]
    public string Description = "";

    [Tooltip("What happens once the last step has been completed"), SerializeReference, ValidateInput(nameof(ValidateOutcome)), MaybeNull]
    public IInteraction Outcome;

    [InlineEditor, ListDrawerSettings(CustomAddFunction = nameof(CustomAddFunction), CustomRemoveElementFunction = nameof(CustomRemoveFunction), DefaultExpandedState = true)]
    public QuestStep[] Steps = Array.Empty<QuestStep>();

    [HideLabel, NonSerialized, ShowInInspector, TextArea, BoxGroup("Debug")]
    public string _debugTxt = "";

    [Button, BoxGroup("Debug")]
    public void CheckIfAllStepsCanBeCompleted()
    {
        #if UNITY_EDITOR
        _debugTxt = "";

        var stepsById = new Dictionary<string, QuestStep>();
        foreach (var questStep in Steps)
        {
            if (UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(questStep.GetInstanceID(), out string stringGUID, out long fileId) == false)
            {
                _debugTxt = "Could not find GUID";
                return;
            }
            stepsById.Add(fileId.ToString(), questStep);
        }

        _debugTxt = "Hold on while we check in the background ...";
        Task.Run(AsyncCheck);
        void AsyncCheck()
        {
            var left = stepsById.ToDictionary(x => x.Key, y => y.Value);

            var regex = new Regex($@"type: {{class: {nameof(CompleteQuestStep)}[^\n]*\s+data:[^\n]*\s+Step: {{fileID: (?<fileId>-?\d+)", RegexOptions.Compiled);

            string completionSource = "";
            foreach (var file in Directory.EnumerateFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories).Concat(Directory.EnumerateFiles(Application.dataPath, "*.asset", SearchOption.AllDirectories)))
            {
                var content = File.ReadAllText(file);
                foreach (Match match in regex.Matches(content))
                {
                    var fileId = match.Groups["fileId"].Value;
                    left.Remove(fileId);
                    completionSource += $"-'{stepsById[fileId].Title}' on '{Path.GetRelativePath(Application.dataPath, file)}'\n";
                }
            }

            if (left.Count == 0)
            {
                _debugTxt = $"All steps have an associated interaction completing them: \n{completionSource}";
            }
            else
            {
                _debugTxt = $"/!\\ Steps '{string.Join(',', left.Select(x => x.Value.Title))}' does not have an associated interaction completing them, others that do are: \n{completionSource}";
            }
            _debugTxt += "\nThis report will not be accurate if scenes or assets haven't been saved.";
        }
        #endif
    }

    QuestStep CustomAddFunction()
    {
        QuestStep newStep = CreateInstance<QuestStep>();
        newStep.Quest = this;
        newStep.name = $"{name} - New Step";
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.AddObjectToAsset(newStep, this);
        UnityEditor.EditorApplication.delayCall += () => UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
        #endif
        return newStep;
    }

    void CustomRemoveFunction(QuestStep step)
    {
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.RemoveObjectFromAsset(step);
        Steps = Steps.Where(x => x != step).ToArray();
        UnityEditor.EditorApplication.delayCall += () => UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
        #endif
    }

    static bool ValidateOutcome(IInteraction interaction, ref string message)
    {
        return interaction == null || interaction.IsValid(out message);
    }
}
