#nullable enable

using System;

namespace Nodalog
{
    using System.Collections;
    using System.Collections.Generic;
    using Unity.VisualScripting;

    [UnitCategory("Nodalog")]
    [UnitTitle("MultiLine")]
    [TypeIcon(typeof(_DummyIconClass))]
    public class NodeMultiLine : Unit
    {
        [DoNotSerialize, PortLabelHidden]
        public ControlInput Enter { get; private set; } = null!;

        [DoNotSerialize, PortLabelHidden]
        public ControlOutput Exit { get; private set; } = null!;

        [Serialize, Inspectable, InspectorTextArea, UnitHeaderInspectable, InspectorWide]
        public DialogIssues Issues = new();

        [Serialize, Inspectable, InspectorTextArea, UnitHeaderInspectable, InspectorWide]
        public Interlocutor? Interlocutor;

        [Serialize, Inspectable, InspectorTextArea, UnitHeaderInspectable, InspectorWide]
        public LineData[] Lines = Array.Empty<LineData>();

        protected override void Definition()
        {
            Enter = ControlInputCoroutine(nameof(Enter), DialogFlow);
            Exit = ControlOutput(nameof(Exit));
            Succession(Enter, Exit);
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

        protected IEnumerator DialogFlow(Flow flow)
        {
            NodeStart.Starts.TryGetValue(flow, out var comp);
            var ui = comp.ui;
            foreach (var line in Lines)
            {
                foreach (var yield in NodeLine.LinePresenter(ui, line.RawString, Interlocutor))
                {
                    yield return yield;
                }
            }

            yield return Exit;
        }

        void ValidateConnections()
        {
            Issues.Issues.Clear();
            CheckStart();
            CheckEnd();

            void CheckStart()
            {
                // Check for dialog start
                var toCheck = new Queue<Unit>();
                var checkedUnits = new HashSet<Unit>(); // To ensure we don't end up in an infinite loop
                toCheck.Enqueue(this);

                while (toCheck.TryDequeue(out var unit))
                {
                    foreach (var port in unit.controlInputs)
                    {
                        int thisPortConnectionCount = 0;
                        foreach (var connection in port.validConnections)
                        {
                            thisPortConnectionCount++;
                            if (connection.source.unit is NodeStart)
                                continue;
                            if (connection.source.unit is Unit u && checkedUnits.Add(u))
                                toCheck.Enqueue(u);
                        }

                        if (thisPortConnectionCount == 0)
                        {
                            Issues.Issues.Add(new(){ Text = $"Requires a {nameof(NodeStart)} set before hand", Type = DialogIssues.Type.Error});
                        }
                    }
                }
            }

            void CheckEnd()
            {
                // Check for dialog end
                var toCheck = new Queue<Unit>();
                var checkedUnits = new HashSet<Unit>(); // To ensure we don't end up in an infinite loop
                toCheck.Enqueue(this);

                while (toCheck.TryDequeue(out var unit))
                {
                    foreach (var port in unit.controlOutputs)
                    {
                        int thisPortConnectionCount = 0;
                        foreach (var connection in port.validConnections)
                        {
                            thisPortConnectionCount++;
                            if (connection.destination.unit is NodeEnd)
                                continue;

                            if (connection.destination.unit is Unit u && checkedUnits.Add(u))
                                toCheck.Enqueue(u);
                        }

                        if (thisPortConnectionCount == 0)
                        {
                            Issues.Issues.Add(new(){ Text = $"Requires a {nameof(NodeEnd)} set before reaching the end of this branch", Type = DialogIssues.Type.Error});
                        }
                    }
                }
            }
        }
    }
}