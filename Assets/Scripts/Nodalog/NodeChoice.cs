#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.VisualScripting;

namespace Nodalog
{
    [UnitCategory("Nodalog")]
    [UnitTitle("Choice")]
    [TypeIcon(typeof(IList))]
    public class NodeChoice : Unit
    {
        [DoNotSerialize, PortLabelHidden]
        public ControlInput? Enter { get; private set; }

        [DoNotSerialize, PortLabelHidden]
        public ReadOnlyCollection<ControlOutput> ChoiceExits = null!;

        [Serialize, Inspectable, InspectorTextArea(maxLines = 1000), UnitHeaderInspectable, InspectorWide]
        public string[] Choices = Array.Empty<string>();

        [DoNotSerialize]
        public ReadOnlyCollection<ValueInput> ChoiceVisible = null!;

        protected override void Definition()
        {
            Enter = ControlInputCoroutine(nameof(Enter), DialogFlow);

            var choices = new List<ValueInput>();
            var exits = new List<ControlOutput>();

            ChoiceVisible = choices.AsReadOnly();
            ChoiceExits = exits.AsReadOnly();

            for (var i = 0; i < Choices.Length; i++)
            {
                // Right now we can't use the choice's text as label as the system uses labels to identify and assign connections
                var text = i.ToString();
                //var text = Choices[i].RawString;
                //text = string.IsNullOrEmpty(text) ? i.ToString() : text;
                var input = ValueInput(text, true);
                choices.Add(input);

                var exit = ControlOutput(text);
                exits.Add(exit);
                Succession(Enter, exit);
            }
        }

        protected IEnumerator DialogFlow(Flow flow)
        {
            NodeStart.Starts.TryGetValue(flow, out var comp);
            var choices = new ChoiceData[Choices.Length];
            for (var i = 0; i < choices.Length; i++)
                choices[i] = new(Choices[i], flow.GetValue<bool>(ChoiceVisible[i]));

            var awaitable = comp.ui.ChoicePresentation(choices);
            while (awaitable.GetAwaiter().IsCompleted == false)
                yield return null;
            var index = Array.IndexOf(choices, awaitable.GetAwaiter().GetResult());

            yield return ChoiceExits[index];
        }
    }
}