#nullable enable
using System.Linq;

namespace Nodalog
{
    using System.Collections.Generic;
    using UnityEngine;
    using Unity.VisualScripting;

    [UnitCategory("Nodalog")]
    [UnitTitle("Start")]
    [TypeIcon(typeof(DialogUIComponent))]
    [IncludeInSettings(true)]
    public class NodeStart : Unit
    {
        public static Dictionary<Flow, (UIBase ui, bool fromPrefab)> Starts = new();

        [DoNotSerialize, PortLabelHidden]
        public ControlInput? Enter { get; private set; }

        [DoNotSerialize, PortLabelHidden]
        public ControlOutput? Exit { get; private set; }

        [Serialize, Inspectable, InspectorTextArea, UnitHeaderInspectable, InspectorWide]
        public DialogIssues Issues = new();

        [DoNotSerialize]
        public ValueInput UI;

        protected override void Definition()
        {
            Exit = ControlOutput(nameof(Exit));
            Enter = ControlInput(nameof(Enter), flow =>
            {
                if (Starts.ContainsKey(flow) == false)
                {
                    var ui = flow.GetValue<UIBase>(UI);
                    if (ui.gameObject.scene.rootCount == 0) // This is a prefab, create a copy and use that instead
                        Starts[flow] = (Object.Instantiate(ui), true);
                    else
                        Starts[flow] = (ui, false);
                    ui.StartDialogPresentation();
                }
                return Exit;
            });
            Succession(Enter, Exit);

            UI = ValueInput<UIBase>(nameof(UI), null!);
        }

        // Workaround to ensure errors are shown and updated whenever connections changes
        [DoNotSerialize]
        public override bool isControlRoot
        {
            get
            {
#if UNITY_EDITOR
                ValidateConnections();
#endif
                return false;
            }
            protected set
            {

            }
        }

        void ValidateConnections()
        {
            Issues.Issues.Clear();

            // Check for coroutine as input
            var toCheck = new Queue<Unit>();
            var checkedUnits = new HashSet<Unit>(); // To ensure we don't end up in an infinite loop
            toCheck.Enqueue(this);

            while (toCheck.TryDequeue(out var unit))
            {
                if (unit is IEventUnit eventUnit && eventUnit.coroutine == false)
                {
                    Issues.Issues.Add(new(){ Text = $"Must enable 'Coroutine' on {eventUnit.GetType()}", Type = DialogIssues.Type.Error});
                }

                foreach (var port in unit.controlInputs)
                {
                    foreach (var connection in port.validConnections)
                    {
                        if (connection.source.unit is NodeStart)
                            continue;
                        if (connection.source.unit is Unit u && checkedUnits.Add(u))
                            toCheck.Enqueue(u);
                    }
                }
            }

            if (defaultValues[nameof(UI)] == null && UI.validConnections.Any() == false)
            {
                Issues.Issues.Add(new(){ Type = DialogIssues.Type.Error, Text = "UI field must be set" });
            }
        }
    }
}