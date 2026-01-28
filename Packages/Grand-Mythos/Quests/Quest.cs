using Sirenix.OdinInspector;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[CreateAssetMenu(menuName = "Quest")]
public class Quest : IdentifiableScriptableObject
{
    [TextArea, Space]
    public string Description = "";

    [Tooltip("What happens once the last step has been completed"), SerializeReference, ValidateInput(nameof(ValidateOutcome))]
    public IInteraction? Outcome;

    [InlineEditor, ListDrawerSettings(CustomAddFunction = nameof(CustomAddFunction), CustomRemoveElementFunction = nameof(CustomRemoveFunction), DefaultExpandedState = true)]
    public QuestStep[] Steps = Array.Empty<QuestStep>();

    public bool Discovered
    {
        get => GameManager.Instance?.DiscoveredQuests.Contains(this) ?? false;
        set
        {
            if (value)
                GameManager.Instance.DiscoveredQuests.Add(this);
            else
                GameManager.Instance.DiscoveredQuests.Remove(this);
        }
    }

    public bool Completed => Steps.Length == 0 || Steps[^1].Completed;

    [Button, ButtonGroup("Debug")]
    void CheckIfAllStepsCanBeCompleted()
    {
        #if UNITY_EDITOR

        int progressId = UnityEditor.Progress.Start(nameof(CheckIfAllStepsCanBeCompleted));

        var stepsById = new Dictionary<string, QuestStep>();
        foreach (var questStep in Steps)
        {
            if (UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(questStep.GetInstanceID(), out string stringGUID, out long fileId) == false)
            {
                UnityEditor.EditorUtility.DisplayDialog(nameof(CheckIfAllStepsCanBeCompleted), "Could not find GUID", "Ok");
                return;
            }
            stepsById.Add(fileId.ToString(), questStep);
        }

        Task.Run(AsyncCheck);
        void AsyncCheck()
        {
            try
            {
                var left = stepsById.ToDictionary(x => x.Key, y => y.Value);

                var regexInput = @"type: {class: " + nameof(CompleteQuestStep) + @"[^\n]*\s+data:[^\n]*\s+Step: {fileID: (?<fileId>-?\d+)";
                var regex = new Regex(regexInput, RegexOptions.Compiled);

                var regexVisualScriptNodesInput = @"{""name"":""" + nameof(QuestStep.Completed) + @"""[^}]*""targetType"":""" + nameof(QuestStep) + @""",""targetTypeName"":""" + nameof(QuestStep) + @"""[^}]*},""defaultValues"":{""target"":{""\$content"":(?<index>\d+),[^\n]*?""Unity\.VisualScripting\.(?<type>(S|G)etMember)""";
                var regexVisualScriptNodes = new Regex(regexVisualScriptNodesInput, RegexOptions.Compiled);
                var regexVisualScriptFiles = new Regex(@"\n\s+- {fileID: (?<fileId>-?\d+)", RegexOptions.Compiled);

                string completionSource = "";

                UnityEditor.Progress.Report(progressId, 0f, "Collecting files");
                var list = Directory.EnumerateFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories).Concat(Directory.EnumerateFiles(Application.dataPath, "*.asset", SearchOption.AllDirectories)).ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    var file = list[i];
                    UnityEditor.Progress.Report(progressId, i / (float)list.Count, $"Parsing {Path.GetFileName(file)}");
                    var content = File.ReadAllText(file);
                    foreach (Match match in regex.Matches(content))
                    {
                        var fileId = match.Groups["fileId"].Value;
                        left.Remove(fileId);
                        if (stepsById.TryGetValue(fileId, out var step))
                            completionSource += $"-'{step.Title}' on '{Path.GetRelativePath(Application.dataPath, file)}'\n";
                    }

                    List<int> indexOfMatches = new List<int>();
                    foreach (Match match in regexVisualScriptNodes.Matches(content))
                    {
                        if (match.Groups["type"].Value == "GetMember")
                        {
                            var index = match.Groups["index"].Value;
                            indexOfMatches.Add(int.Parse(index));
                        }
                    }

                    if (indexOfMatches.Count > 0)
                    {
                        Match[] matches = regexVisualScriptFiles.Matches(content).ToArray();
                        foreach (var indexOfMatch in indexOfMatches)
                        {
                            var fileId = matches[indexOfMatch].Groups["fileId"].Value;
                            left.Remove(fileId);
                            if (stepsById.TryGetValue(fileId, out var step))
                                completionSource += $"-'{step.Title}' on '{Path.GetRelativePath(Application.dataPath, file)}'\n";
                        }
                    }
                }

                string debugTxt;
                if (left.Count == 0)
                {
                    debugTxt = $"All steps have an associated interaction completing them:\n{completionSource}";
                }
                else
                {
                    debugTxt = $"/!\\ Uncompletable:\n'{string.Join("\n- ", left.Select(x => x.Value.Title))}'\nMake sure the step(s) above have an interaction or visual script completing them\n\nCompletable:\n{completionSource}";
                }

                debugTxt += "\n\nThis report will not be accurate if scenes or assets haven't been saved.";
                UnityEditor.EditorApplication.delayCall += () => { UnityEditor.EditorUtility.DisplayDialog(nameof(CheckIfAllStepsCanBeCompleted), debugTxt, "Ok"); };
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                UnityEditor.Progress.Remove(progressId);
            }
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

    static bool ValidateOutcome(IInteraction? interaction, ref string? message)
    {
        return interaction == null || interaction.IsValid(out message);
    }
}
