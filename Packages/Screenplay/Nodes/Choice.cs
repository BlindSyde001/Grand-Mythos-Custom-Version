using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using YNode;

namespace Screenplay.Nodes
{
    [NodeTint(60, 60, 60)]
    public class Choice : ScreenplayNode, IAction, ILocalizableNode
    {
        [ListDrawerSettings(ShowFoldout = false), LabelText(" ")]
        public ChoiceInstance[] Choices =
        {
            new()
            {
                Text = new("Choice A"),
            },
            new()
            {
                Text = new("Choice B"),
            }
        };

        public bool TestPrerequisite(HashSet<IPrerequisite> visited) => visited.Contains(this);

        public IEnumerable<IAction> Followup()
        {
            foreach (var instance in Choices)
            {
                if (instance.Action != null)
                    yield return instance.Action;
            }
        }

        public void FastForward(IContext context) { }

        public IEnumerable<Signal> Execute(IContext context)
        {
            var choicesThin = Choices.Select(x => new Data(x.Prerequisite?.TestPrerequisite(context.Visited) ?? true, x.Text.Content)).ToArray();
            if (context.GetDialogUI() is {} ui == false)
            {
                Debug.LogWarning($"{nameof(ScreenplayGraph.DialogUIPrefab)} has not been set, no interface to present this {nameof(Choice)} on");
                for (int i = 0; i < choicesThin.Length; i++)
                {
                    if (choicesThin[i].Enabled)
                    {
                        if (Choices[i].Action is not null)
                            yield return Signal.BreakInto(Choices[i].Action);
                        yield break;
                    }
                }

                yield break;
            }

            ui.StartDialogPresentation();
            var choice = ui.ChoicePresentation(choicesThin);
            while (choice.Selection is null)
                yield return Signal.NextFrame;
            ui.EndDialogPresentation();

            var index = Array.IndexOf(choicesThin, choice.Selection);

            if (Choices[index].Action is {} a)
                yield return Signal.BreakInto(a);
        }

        public void SetupPreview(IPreviewer previewer, bool fastForwarded)
        {
            if (fastForwarded == false)
                previewer.PlaySafeAction(this);
        }

        public IEnumerable<LocalizableText> GetTextInstances()
        {
            foreach (var instance in Choices)
            {
                yield return instance.Text;
            }
        }

        public override void CollectReferences(List<GenericSceneObjectReference> references) { }

        [Serializable]
        public struct ChoiceInstance
        {
            [FormerlySerializedAs("Requirement")] [Input(Stroke = NoodleStroke.Dashed), SerializeReference, LabelWidth(20), HorizontalGroup(width:90), Tooltip("Which nodes need to be visited for this choice to become selectable")]
            public IPrerequisite? Prerequisite;

            [Output, SerializeReference, LabelWidth(10), HorizontalGroup, Tooltip("What will be executed when this choice is selected")]
            public IAction? Action;

            [HideLabel, InlineProperty]
            public LocalizableText Text;
        }

        public record Data(bool Enabled, string Text)
        {
            /// <summary>
            /// Whether this choice has its prerequisite fulfilled
            /// </summary>
            public bool Enabled { get; } = Enabled;

            public string Text { get; } = Text;
        }
    }
}
